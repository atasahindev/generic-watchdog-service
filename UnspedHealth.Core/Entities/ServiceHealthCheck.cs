using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnspedHealth.Core.Entities
{
    [Table("UGM_ServiceHealthChecks")]
    public class ServiceHealthCheck
    {
        [Key]
        public Guid ID { get; set; }

        [Required, StringLength(250)]
        public string ApplicationName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string ServerIP { get; set; } = string.Empty;

        [StringLength(500)]
        public string ServicePath { get; set; } = string.Empty;
        public DateTime? LastHeartbeat { get; set; }
        public int Status { get; set; }
        public int? CheckInterval { get; set; }
        public bool IsNotifyEnabled { get; set; } = false;
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? WindowsServiceName { get; set; }
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
