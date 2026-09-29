using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.ServiceProcess;
using UnspedHealth.API.Persistence;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Entities;
using UnspedHealth.Core.Interfaces;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Core.Services
{
    public class HealthService: IHealthService
    {

        private readonly AppDbContext _db;
        private readonly ILogger<HealthService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HealthService(AppDbContext db, ILogger<HealthService> logger, IHttpClientFactory httpClientFactory)
        {
            _db = db;

            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ReceiveHeartbeatAsync(Guid serviceID)
        {
            try
            {
                var service = await _db.ServiceHealthChecks.FindAsync(serviceID);
                if (service == null) return false;

                service.LastHeartbeat = DateTime.Now;

                await _db.SaveChangesAsync();
                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> ReceiveHeartbeatAsync(HeartbeatRequest req)
        {
            try
            {
                var service = await _db.ServiceHealthChecks.FindAsync(req.ServiceID);
                if (service == null) return false;

                service.LastHeartbeat = DateTime.Now;

                if (req.IsError)
                {
                    _db.HealthCheckLogs.Add(new HealthCheckLog
                    {
                        HealthCheckID = service.ID,
                        Status = (int)ServiceStatus.Degraded,
                        Message = $"[HATA] {service.ApplicationName} servisi hata bildirimi gönderdi. Mesaj: {req.Message}",
                        CreatedAt = DateTime.Now
                    });
                }

                await _db.SaveChangesAsync();
                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<ServiceHealthCheck>> CheckAllServicesAsync(int workerIntervalInSeconds)
        {
            try
            {
                var response = new List<ServiceHealthCheck>();
                var now = DateTime.Now;

                var services = await _db.ServiceHealthChecks
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.MonitoringMode == (int)MonitoringMode.Passive) 
                    .ToListAsync();

                foreach (var service in services)
                {
                    // uygulamanın db'de kendi ayarı yoksa worker çalışma süresini baz alıyoruz. (Default: 300sn)
                    int appHeartbeatInterval = service.CheckInterval.GetValueOrDefault(workerIntervalInSeconds);
                    int toleranceInterval = appHeartbeatInterval + (workerIntervalInSeconds / 2) + 30;

                    bool isTimedOut = service.LastHeartbeat == null || (now - service.LastHeartbeat.Value).TotalSeconds > toleranceInterval;

                    // DURUM 1: Servis zaman aşımına uğradıysa ve durumu hala "Healthy" veya "Stateless" ise.
                    if (isTimedOut && service.Status != (int)ServiceStatus.Down)
                    {
                        service.Status = (int)ServiceStatus.Down;

                        _logger.LogWarning("[SERVICE_DOWN] | App: {ApplicationName} | Server: {ServerIP} | LastSeen: {LastHeartbeat}", service.ApplicationName, service.ServerIP, service.LastHeartbeat);

                        var logEntry = new HealthCheckLog
                        {
                            HealthCheckID = service.ID,
                            Status = (int)ServiceStatus.Down,
                            Message = $"[KRİTİK KESİNTİ] {service.ApplicationName} servisi durdu. Sunucu: {service.ServerIP}",
                            CreatedAt = now
                        };

                        _db.HealthCheckLogs.Add(logEntry);

                        // TO-DO:
                        // Bildirim Gönder (IsNotifyEnabled kontrolü burada yapılır)
                        //if (service.IsNotifyEnabled)
                        //    await TriggerNotificationAsync(service, "DURDU");

                        response.Add(service);
                    }

                    // DURUM 2: Servis daha önce sağlıklı değildi (Down || Degraded || Stateless).
                    else if (!isTimedOut && (service.Status == (int)ServiceStatus.Down || service.Status == (int)ServiceStatus.Degraded || service.Status == (int)ServiceStatus.Stateless))
                    {
                        var oldStatus = (ServiceStatus)service.Status;
                        service.Status = (int)ServiceStatus.Healthy;

                        string logMessage = oldStatus == ServiceStatus.Stateless
                            ? $"[SERVICE_STARTED] {service.ApplicationName} ilk sinyalini gönderdi ve çalışmaya başladı."
                            : $"[SERVICE_RECOVERED] {service.ApplicationName} servisi tekrar çalışmaya başladı.";

                        _logger.LogInformation(logMessage);

                        _db.HealthCheckLogs.Add(new HealthCheckLog
                        {
                            HealthCheckID = service.ID,
                            Status = (int)ServiceStatus.Healthy,
                            Message = logMessage,
                            CreatedAt = now
                        });
                    }
                }

                int affectedRows = await _db.SaveChangesAsync();

                if (affectedRows > 0)
                    _logger.LogInformation("[VERİTABANI] {Count} adet log ve durum değişikliği başarıyla kaydedildi.", affectedRows);


                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HATA] | Servis denetimi sırasında beklenmedik bir hata oluştu!");
                throw;
            }
        }
        public async Task<List<ServiceHealthCheck>> CheckAllExternalPythonServicesAsync()
        {
            // 1. Veritabanından tipi "Python/Docker" olan aktif servisleri çek (Örn: ServiceType == 2)
            var pythonServices = await _db.ServiceHealthChecks
                    .Where(p => p.IsActive && p.MonitoringMode == (int)MonitoringMode.Active)
                    .ToListAsync();

            var downedServices = new List<ServiceHealthCheck>();
            var now = DateTime.Now;

            var httpClient = _httpClientFactory.CreateClient();
            var logEntries = new ConcurrentBag<HealthCheckLog>();

            // 2. Task listesi oluşturarak paralel başlatıyoruz
            var tasks = pythonServices.Select(async service =>
            {

                if (service.ApiPort == null || string.IsNullOrEmpty(service.HealthEndpoint))
                {
                    _logger.LogWarning("[PYTHON_API_CONFIG] | {App} (@{IP}): API portu veya health endpoint bilgisi eksik.", service.ApplicationName, service.ServerIP);
                    return null;
                }

                RestClientOptions options = new RestClientOptions(service.HealthEndpoint);
                var client = new RestClient(httpClient, options);
                var request = new RestRequest("", Method.Get);

                request.Timeout = TimeSpan.FromSeconds(15);

                try
                {
                    var response = await client.ExecuteAsync<PythonServiceHealthResp>(request);

                    if (response.IsSuccessful && response.Data?.Status.ToUpper() == "OK")
                    {
                        service.LastHeartbeat = now;
                        service.Status = (int)ServiceStatus.Healthy;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("[PYTHON_API_ERROR] | {App} (@{IP}): {Msg}", service.ApplicationName, service.ServerIP, ex.Message);
                }

                // Buraya geldiyse servis down durumda demektir.
                service.Status = (int)ServiceStatus.Down;

                logEntries.Add(new HealthCheckLog
                {
                    HealthCheckID = service.ID,
                    Status = (int)ServiceStatus.Down,
                    Message = $"[KRİTİK] Python API yanıt vermiyor. URL: {service.HealthEndpoint}",
                    CreatedAt = now
                });

                return service;
            });

            var results = await Task.WhenAll(tasks);

            if (logEntries.Any())
            {
                await _db.HealthCheckLogs.AddRangeAsync(logEntries);
            }

            await _db.SaveChangesAsync();

            return results.Where(s => s != null).ToList()!;
        }
        public async Task<(bool IsOnline, string Message)> GetSingleServiceStatusAsync(Guid serviceId)
        {
            try
            {
                var service = await _db.ServiceHealthChecks
                    .FirstOrDefaultAsync(s => s.ID == serviceId);

                if (service == null)
                    return (false, "Servis veritabanında bulunamadı.");

                if (!string.IsNullOrEmpty(service.HealthEndpoint) && service.MonitoringMode == (int)MonitoringMode.Active)
                {
                    try
                    {
                        using var client = _httpClientFactory.CreateClient();
                        client.Timeout = TimeSpan.FromSeconds(8);

                        var response = await client.GetAsync(service.HealthEndpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            return (true, "Healthy (API Response OK)");
                        }

                        return (false, $"Unhealthy (Status Code: {response.StatusCode})");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"API Connection Error: {ex.Message}");
                    }
                }

                if (service.MonitoringMode == (int)MonitoringMode.Passive)
                {
                    // 3. EĞER SERVİS HEARTBEAT TABANLIYSA (PASİF): Son sinyal zamanını kontrol et
                    // Tolerans payı: CheckInterval'in üzerine 10 saniyelik bir ağ gecikmesi eklemek mantıklıdır
                    var interval = service.CheckInterval ?? 300;
                    var threshold = DateTime.Now.AddSeconds(-(interval + 10));

                    bool isAlive = service.LastHeartbeat.HasValue && service.LastHeartbeat >= threshold;

                    if (isAlive)
                    {
                        return (true, "Healthy (Heartbeat Active)");
                    }

                    return (false, $"No Heartbeat detected since {service.LastHeartbeat?.ToString("HH:mm:ss") ?? "N/A"}");
                }

                return (false, "Servis durumu belirlenemedi.");

            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
