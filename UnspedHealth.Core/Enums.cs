using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core
{
    public class Enums
    {
        public enum ServiceStatus
        {
            Stateless = 1,
            Healthy = 2,
            Down = 3,
            Degraded = 4
        }
        public enum PushNotificationType
        {
            [Display(Name = "E-Posta")]
            Email = 1,
            [Display(Name = "SMS Bildirimi")]
            SMS = 2,
            [Display(Name = "WhatsApp Mesajı")]
            WhatsApp = 3,
            [Display(Name = "Slack Kanalı")]
            Slack = 4
        }

        public enum AlertLevel
        {
            Info = 1,
            Warning = 2,
            Critical = 3,
            Error = 4
        }

        public enum MonitoringMode
        {
            [Description("Servis sinyal gönderir - (Heartbeat)")]
            Passive = 1,

            [Description("Watchdog servise istek atar - (API Check)")]
            Active = 2
        }
    }
}
