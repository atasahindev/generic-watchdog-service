using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnspedHealth.Core.Entities
{
    [Table("UGM_AllServers")]
    public class Server
    {
        [Key]
        public Guid ID { get; set; }

        // SQL'de "Checked" olduğu için [Required] kaldırılmalı ve string? yapılmalı
        [StringLength(100)]
        public string? ServerName { get; set; }

        [Required, StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // SQL'de "Checked" olan tüm DateTime alanları zaten DateTime? (Bu kısım doğru)
        public DateTime? LastPingTime { get; set; }

        // Eğer veritabanında eski kayıtlarda CreatedAt NULL ise bu da DateTime? olmalı
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public int? Status { get; set; }

        public DateTime? LastDownTime { get; set; }
        public DateTime? LastUpTime { get; set; }
    }
}
