using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.DTOs
{
    public class AddServerRequest
    {
        public string? ServerName { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int? Status { get; set; }
    }
}
