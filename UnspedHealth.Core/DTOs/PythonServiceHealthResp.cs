using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.DTOs
{
    public class PythonServiceHealthResp
    {
        public string Status { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}
