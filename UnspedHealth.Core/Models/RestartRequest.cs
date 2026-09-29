using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Models
{
    public record RestartRequest(
        string ProcessName,
        string ExecutionPath,
        string? Arguments = null
    );
}
