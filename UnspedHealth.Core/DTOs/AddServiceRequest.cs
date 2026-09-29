using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.DTOs
{
    public class AddServiceRequest
    {
        [Required, StringLength(250)]
        public string ApplicationName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string ServerIP { get; set; } = string.Empty;

        [StringLength(500)]
        public string ServicePath { get; set; } = string.Empty;
        public int? CheckInterval { get; set; }
        public bool IsNotifyEnabled { get; set; } = false;
        public string? Description { get; set; }
        public string? WindowsServiceName { get; set; }
        public bool IsActive { get; set; }
        public bool? IsWindowsService { get; set; }
        public bool? AutoRestartEnabled { get; set; }
        public int? MonitoringMode { get; set; }
        public int? ApiPort { get; set; }
        public string? ContainerName { get; set; }
        public string? BatchFilePath { get; set; }
        public string? Type { get; set; }
        public string? HealthEndpoint { get; set; }
        public string? Contacts { get; set; }
    }
}
