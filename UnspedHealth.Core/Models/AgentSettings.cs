using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Models
{
    public static class AgentSettings
    {
        public static int DefaultPort { get; set; }
        public static string ApiKey { get; set; } = string.Empty;
    }
}
