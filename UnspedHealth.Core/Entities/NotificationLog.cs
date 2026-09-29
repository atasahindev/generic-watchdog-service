using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnspedHealth.Core.Entities
{
    [Table("UGM_ServiceNotificationLogs")]
    public class NotificationLog
    {
        public int ID { get; set; }
        public Guid? HealthCheckID { get; set; }
        public string? NotificationType { get; set; }

        [Required]
        public string Recipient { get; set; } = string.Empty;
        public string? Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        public bool IsSuccess { get; set; }
    }
}
