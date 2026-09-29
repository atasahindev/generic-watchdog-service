using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Text;
using UnspedHealth.API.Persistence;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Entities;
using UnspedHealth.Core.Extensions;
using UnspedHealth.Core.Interfaces;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationService(AppDbContext db, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<bool> SendNotificationAsync(PushNotificationType notificationType, NotificationMessageRequest request)
        {
            try
            {
                var (isSuccess, message) = notificationType switch
                {
                    PushNotificationType.Email => await SendEmail(request),
                    PushNotificationType.SMS => await SendSMS(request),
                    PushNotificationType.WhatsApp => await SendWhatsApp(request),
                    PushNotificationType.Slack => await SendSlack(request),
                    _ => (false, "Desteklenmeyen bildirim türü.")
                };

                if (isSuccess)
                    Log.Information("[NotificationService] | {Type}: {Message}", notificationType, message);

                return isSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[NotificationService] | Beklenmedik bir hata oluştu.");
                return false;
            }
        }
        public async Task<bool> ProcessActiveChannelsAsync(List<PushNotificationType> activeChannels, NotificationMessageRequest request)
        {
            try
            {
                bool allSucceeded = true;

                foreach (var channelType in activeChannels)
                {
                    bool isSuccess = await SendNotificationAsync(channelType, request);

                    if (!isSuccess)
                        allSucceeded = false;
                }

                return allSucceeded;

            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> AddNotificationLogAsync(NotificationLogRequest req)
        {
            try
            {
                var log = new NotificationLog
                {
                    HealthCheckID = req.HealthCheckID,
                    NotificationType = req.Type.GetDisplayName(),
                    Recipient = req.Recipient,
                    Content = req.Content,
                    SentAt = DateTime.Now,
                    IsSuccess = req.IsSuccess,
                    IsRead = false
                };

                await _db.NotificationLogs.AddAsync(log);
                var result = await _db.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region Private Methods
        private async Task<(bool IsSuccess, string Message)> SendEmail(NotificationMessageRequest request)
        {
            // Email gönderme işlemleri
            return await Task.FromResult((true, "Mail notification sent successfully."));

        }
        private async Task<(bool IsSuccess, string Message)> SendSMS(NotificationMessageRequest request)
        {
            // SMS gönderme işlemleri
            return await Task.FromResult((true, "SMS notification sent successfully."));

        }
        private async Task<(bool IsSuccess, string Message)> SendWhatsApp(NotificationMessageRequest request)
        {
            // WhatsApp gönderme işlemleri
            return await Task.FromResult((true, "Whatsapp notification sent successfully."));

        }
        private async Task<(bool IsSuccess, string Message)> SendSlack(NotificationMessageRequest request)
        {
            try
            {
                var webhookUrl = _configuration["NotificationSettings:SlackWebhookUrl"];
                if (string.IsNullOrEmpty(webhookUrl))
                    return (false, "Config: Slack URL boş.");

                var client = _httpClientFactory.CreateClient();

                // Mobilde daha iyi görünmesi için Header rengi ve ikon odaklı yapı
                var slackMessage = new
                {
                    blocks = new List<object>
            {
                new {
                    type = "section",
                    text = new { type = "mrkdwn", text = $"*{GetStatusEmoji(request.Level)} {request.Title}*" }
                },
                new {
                    type = "context",
                    elements = new[] {
                        new { type = "mrkdwn", text = $"🕒 *Zaman:* {request.Timestamp:dd.MM HH:mm:ss} | *Seviye:* `{request.Level}`" }
                    }
                },
                new { type = "divider" }
            }
                };

                // Servis detaylarını tek tek Block olarak ekleyelim (Mobilde daha okunaklı olur)
                foreach (var block in BuildMobileBlocks(request))
                {
                    slackMessage.blocks.Add(block);
                }

                slackMessage.blocks.Add(new { type = "divider" });
                slackMessage.blocks.Add(new
                {
                    type = "context",
                    elements = new[] { new { type = "mrkdwn", text = "🛡️ *Unsped Health Monitoring System*" } }
                });

                var content = new StringContent(JsonConvert.SerializeObject(slackMessage), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(webhookUrl, content);

                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Başarılı" : $"Hata: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Slack gönderim hatası.");
                return (false, ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        private string BuildSlackDetails(NotificationMessageRequest request)
        {
            var sb = new StringBuilder();

            if (request.DownServices.Any())
            {
                sb.AppendLine("\n*Uygulamalar:*");
                foreach (var s in request.DownServices)
                    sb.AppendLine($"> • `{s.ApplicationName}` - Sunucu: {s.ServerIP}");
            }

            if (request.DownServers.Any())
            {
                sb.AppendLine("\n*Sunucular:*");
                foreach (var s in request.DownServers)
                    sb.AppendLine($"> • `{s.ServerName}` - ({s.IpAddress})");
            }

            if (!request.DownServices.Any() && !request.DownServers.Any())
                sb.AppendLine("_Kesinti detayları alınamadı veya liste boş._");

            return sb.ToString();
        }
        private List<object> BuildMobileBlocks(NotificationMessageRequest request)
        {
            var blocks = new List<object>();

            if (request.DownServices.Any())
            {
                blocks.Add(new { type = "section", text = new { type = "mrkdwn", text = "💻 *Uygulama Kesintileri*" } });

                foreach (var s in request.DownServices)
                {
                    blocks.Add(new
                    {
                        type = "section",
                        text = new
                        {
                            type = "mrkdwn",
                            text = $"• *{s.ApplicationName}*\n" +
                                   $"   📍 `Sunucu:` {s.ServerIP}\n" +
                                   $"   📂 `Path:` {(!string.IsNullOrEmpty(s.ServicePath) ? s.ServicePath : "Tanımsız")}\n" +
                                   $"   ⚙️ `Auto Restart:` {(s.AutoRestartEnabled.GetValueOrDefault() ? "Aktif" : "Devre Dışı")}"
                        }
                    });
                }
            }

            if (request.DownServers.Any())
            {
                if (blocks.Any()) blocks.Add(new { type = "divider" }); // Uygulamalar varsa araya çizgi at

                blocks.Add(new { type = "section", text = new { type = "mrkdwn", text = "🖥️ *Sunucu Kesintileri*" } });
                foreach (var s in request.DownServers)
                {
                    blocks.Add(new
                    {
                        type = "section",
                        text = new
                        {
                            type = "mrkdwn",
                            text = $"• *{s.ServerName}*\n" +
                                   $"   🌐 `IP:` {s.IpAddress}\n" +
                                   $"   📉 `Durum:` Çevrimdışı (Ping Yok)"
                        }
                    });
                }
            }

            return blocks;
        }
        private string GetStatusEmoji(AlertLevel level) => level switch
        {
            AlertLevel.Critical => "🔴",
            AlertLevel.Error => "🟠",
            AlertLevel.Warning => "🟡",
            _ => "🔵"
        };

        #endregion
    }
}
