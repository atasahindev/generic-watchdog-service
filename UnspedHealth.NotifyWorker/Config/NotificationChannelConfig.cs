using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Worker.Config
{
    public class NotificationChannelConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }
}
