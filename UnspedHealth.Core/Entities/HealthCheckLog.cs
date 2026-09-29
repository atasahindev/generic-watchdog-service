using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnspedHealth.Core.Entities
{
    [Table("UGM_ServiceHealthCheckLogs")]
    public class HealthCheckLog
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public Guid HealthCheckID { get; set; }
        public int? Status { get; set; }
        public string? Message { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
