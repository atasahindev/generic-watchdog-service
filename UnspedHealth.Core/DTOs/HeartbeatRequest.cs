using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.DTOs
{
    /// <summary>
    /// Servislerden gelen sağlık ve durum bildirimlerini taşıyan veri transfer objesi.
    /// </summary>
    public class HeartbeatRequest
    {
        /// <summary>
        /// Sinyal gönderen servisin veritabanındaki benzersiz kimliği.
        /// </summary>
        public Guid ServiceID { get; set; }

        /// <summary>
        /// Servis tarafından gönderilen durum veya hata mesajı.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gönderilen mesajın bir hata (True) mi yoksa bilgi (False) mi olduğunu belirtir.
        /// </summary>
        public bool IsError { get; set; }
    }
}
