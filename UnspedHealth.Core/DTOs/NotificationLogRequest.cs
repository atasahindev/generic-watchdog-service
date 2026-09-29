using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Core.DTOs
{
    public class NotificationLogRequest
    {
        public int ID { get; set; }
        public Guid? HealthCheckID { get; set; }
        public PushNotificationType Type { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string? Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsSuccess { get; set; }
    }
}
