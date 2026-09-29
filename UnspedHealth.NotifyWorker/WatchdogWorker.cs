using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using UnspedHealth.Core;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Entities;
using UnspedHealth.Core.Interfaces;
using UnspedHealth.Core.Services;
using UnspedHealth.Core.Utilities;
using UnspedHealth.Worker.Config;
using UnspedHealth.Worker.Constants;
using UnspedHealth.Worker.Helper;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Worker;

public class WatchdogWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly List<PushNotificationType> _activeChannels; // aktif bildirim kanalları, appsettings içerisinden okunuyor.
    private const int _checkIntervalInSeconds = 300; // 5 dakika.

    public WatchdogWorker(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;

        _activeChannels = (_configuration.GetSection("NotificationSettings:Channels")
            .Get<List<NotificationChannelConfig>>() ?? new List<NotificationChannelConfig>())
            .Where(x => x.IsEnabled && !string.IsNullOrEmpty(x.Type))
            .Where(x =>
            {
                var valid = Enum.TryParse<PushNotificationType>(x.Type, true, out _);
                if (!valid) Log.Warning("[CONFIG] Geçersiz bildirim kanalı tipi atlandı: {Type}", x.Type);
                return valid;
            })
            .Select(x => Enum.Parse<PushNotificationType>(x.Type, true))
            .ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("""
    Tarih: {Time}
    Kontrol Aralığı: {Interval} Saniye
    Mod: Üretim (Production)
    ===========================================================
    """, DateTime.Now, _checkIntervalInSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();
                    var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();
                    var recoveryService = scope.ServiceProvider.GetRequiredService<IRecoveryService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    Log.Information("[TARAMA BAŞLADI] | Sistemler denetleniyor...");

                    // aktif tip => python tabanlı web servisler için, belirli endpoint'e istek atarak api servisinin durumunu kontrol eder.
                    // pasif tip => http endpointi bulunmayan klasik servisler için, son heartbeat isteğine bakarak durumunu kontrol eder.

                    var downedServices = await healthService.CheckAllServicesAsync(_checkIntervalInSeconds);
                    var downedPythonServices = await healthService.CheckAllExternalPythonServicesAsync();
                    var downedServers = await serverService.CheckAllServersAsync();

                    sw.Stop();

                    var allDownedServices = downedPythonServices.Concat(downedServices).ToList();

                    // tespit edilen kesintilerle ilgili bildirim, loglama ve otomatik kurtarma işlemlerini gerçekleştirir.
                    await ProcessResultsAsync(allDownedServices, downedServers, healthService, recoveryService, notificationService);

                    Log.Information("[TARAMA TAMAMLANDI] | Süre: {Duration} ms | Sonraki Tarama: {NextTime}", sw.ElapsedMilliseconds, DateTime.Now.AddSeconds(_checkIntervalInSeconds).ToString("HH:mm:ss"));
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[SİSTEM HATASI] | Watchdog döngüsü beklenmedik bir şekilde kesildi.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_checkIntervalInSeconds), stoppingToken);
        }
    }
    private async Task ProcessResultsAsync(List<ServiceHealthCheck> services, List<Server> servers, IHealthService healthService, IRecoveryService recoveryService, INotificationService notifyService)
    {
        try
        {
            var recoveredIDs = new List<Guid>();

            if (services.Any() || servers.Any())
            {
                Log.Warning("[KESİNTİ TESPİTİ] | Aşağıdaki sistem bileşenlerinde erişim sorunu tespit edildi:");

                foreach (var s in servers) Log.Error("[HOST_OFFLINE]: {Name} ({IP})", s.ServerName, s.IpAddress);

                foreach (var s in services)
                {

                    Log.Error("[APP_OFFLINE]: {AppName} | IP: {IP}", s.ApplicationName, s.ServerIP);

                    // sadece yolu ve ismi belli olan, otomatik restart izni verilmiş servisler için recovery işlemi deneniyor.
                    if (!string.IsNullOrEmpty(s.ServicePath) && !string.IsNullOrEmpty(s.ApplicationName) && !string.IsNullOrEmpty(s.BatchFilePath) && s.AutoRestartEnabled.GetValueOrDefault(false))
                    {
                        Log.Warning("[RECOVERY_ATTEMPT] | {App} için otomatik yeniden başlatma deneniyor...", s.ApplicationName);

                        var recovery = await recoveryService.RestartRemoteProcessAsync(s.ServerIP, s.ApplicationName, s.BatchFilePath);

                        if (recovery.IsSuccess)
                        {
                            Log.Information("[RECOVERY_WAIT] | {App} tetiklendi, 15sn bekleniyor...", s.ApplicationName);
                            await Task.Delay(TimeSpan.FromSeconds(15));

                            var currentStatus = await healthService.GetSingleServiceStatusAsync(s.ID);
                            if (currentStatus.IsOnline)
                            {
                                Log.Information("[RECOVERY_SUCCESS] | {App} başarıyla ayağa kalktı.", s.ApplicationName);
                                recoveredIDs.Add(s.ID);
                            }
                            else
                                Log.Error("[RECOVERY_VERIFY_FAILED] | {App} tetiklendi ama hala erişilemiyor.", s.ApplicationName);
                        }

                        else
                            Log.Error("[RECOVERY_FAILED] | {App} başlatılamadı! Hata: {Message}", s.ApplicationName, recovery.Message);
                    }
                }

                // push notifications.
                string subject = $"{NotificationConstants.DefaultSubject}{services.Count + servers.Count}";
                await SendNotificationsAsync(services, servers, recoveredIDs, notifyService, subject, _activeChannels);
            }

            else
                Log.Information("[STABİL] | Yeni bir kesinti tespit edilmedi. Tüm sistemler sağlıklı veya mevcut kesintiler zaten bildirilmiş durumda.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[PROCESS_RESULTS_EX] | Sonuçlar işlenirken beklenmedik bir hata oluştu.");
        }
    }
    private async Task SendNotificationsAsync(List<ServiceHealthCheck> services, List<Server> servers, List<Guid> recoveredIDs, INotificationService notifyService, string subject, List<PushNotificationType> activeChannels)
    {
        try
        {
            #region NotificationChannel - NotificationService Entegrasyonu

            if (activeChannels.Any())
            {
                NotificationMessageRequest notifyRequest = new NotificationMessageRequest
                {
                    Title = subject,
                    Summary = $"{NotificationConstants.FromName}",
                    DownServices = services,
                    DownServers = servers,
                    Level = AlertLevel.Critical
                };

                await notifyService.ProcessActiveChannelsAsync(activeChannels, notifyRequest);
            }

            #endregion

            #region Mail Gönderimi

            // şuanki yapıda bildirim olarak sadece e-posta ile yapılıyor, ileride SMS veya diğer kanallar eklenebilir. notification servisi içerisinde method gövdeleri oluşturuldu.
            string mailBody = ReplaceHelper.BuildHtmlContentForMail(services, servers, recoveredIDs);

            var extraContacts = services
                .Where(s => !string.IsNullOrWhiteSpace(s.Contacts))
                .SelectMany(s =>
                {
                    if (!string.IsNullOrWhiteSpace(s.Contacts))
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<List<string>>(s.Contacts) ?? new List<string>();
                        }
                        catch (Exception)
                        {
                            return s.Contacts.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                    }

                    return Enumerable.Empty<string>();

                })
                .Select(email => email.Trim())
                .ToList();

            var finalRecipientList = NotificationConstants.ToList
                .Concat(extraContacts)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            bool isSended = await MailHelper.MailSendAsync(
                Kime: finalRecipientList,
                Kimden: NotificationConstants.FromMail,
                Bilgi: null!,
                KimeBCC: NotificationConstants.BccList,
                Konu: subject,
                Govde: mailBody,
                KimeAd: NotificationConstants.FromName,
                Ekler: null!
            );

            if (isSended)
            {
                Log.Information("[BİLDİRİM GÖNDERİLDİ] | İlgili taraflara kesinti bildirimi e-postası başarıyla gönderildi.");

                await notifyService.AddNotificationLogAsync(new NotificationLogRequest
                {
                    Type = PushNotificationType.Email,
                    Content = subject,
                    Recipient = JsonConvert.SerializeObject(finalRecipientList),
                    HealthCheckID = null,
                    IsSuccess = true
                });
            }
            else
            {
                Log.Error("[BİLDİRİM GÖNDERİLEMEDİ] | Kesinti bildirimi e-postası gönderilirken bir hata oluştu.");
            }

            #endregion
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[PUSH_NOTIFY_EX] | Bildirim kanalları veya e-posta gönderimi sırasında teknik bir hata oluştu.");
        }
    }
}