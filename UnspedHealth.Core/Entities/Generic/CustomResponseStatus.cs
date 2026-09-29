using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Entities
{
    public enum CustomResponseStatus
    {
        [JsonProperty(propertyName: "success")]
        Success = 1,
        [JsonProperty(propertyName: "error")]
        Error = 2,
        [JsonProperty(propertyName: "warning")]
        Warning = 3
    }
}
