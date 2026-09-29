using UnspedHealth.Core.Entities;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Core.DTOs
{
    public record HeartbeatNotifyRequest(
        string ServiceName,
        string ServiceUrl,
        Enums.ServiceStatus Status,
        DateTime Timestamp,
        string Message
    );

    public class NotificationMessageRequest
    {
        // Bildirimin başlığı (Örn: "Kritik Sistem Kesintisi!")
        public string Title { get; set; } = string.Empty;

        // Kısa özet (Özellikle SMS ve Push bildirimleri için)
        public string Summary { get; set; } = string.Empty;

        // Teknik detaylar (Slack ve Email için zengin içerik)
        public List<ServiceHealthCheck> DownServices { get; set; } = new();
        public List<Server> DownServers { get; set; } = new();

        // Bildirim önceliği (UI'da rengi belirlemek için kullanılabilir)
        public AlertLevel Level { get; set; } = AlertLevel.Error;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Yardımcı özellik: Toplam kaç kalem sorunlu?
        public int TotalIssueCount => DownServices.Count + DownServers.Count;
    }
}
