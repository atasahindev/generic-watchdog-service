using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.Core.Entities;

namespace UnspedHealth.Core.Interfaces
{
    public interface IReportService
    {
        /// <summary>
        /// Sistemde kayıtlı olan servislerin güncel sağlık durumlarını getirir.
        /// </summary>
        /// <returns></returns>
        Task<CustomResponse<List<ServiceHealthCheck>>> GetAllServiceHealthsAsync();

        /// <summary>
        /// Sistemde kayıtlı olan sunucuların güncel sağlık durumlarını getirir.
        /// </summary>
        /// <returns></returns>
        Task<CustomResponse<List<Server>>> GetAllServerHealthsAsync();
    }
}
