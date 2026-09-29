using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Models
{
    public record AgentResponse(
        bool Success,
        string Message,
        DateTime Timestamp
    );
}
