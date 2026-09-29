using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.Core.DTOs;

namespace UnspedHealth.Core.Interfaces
{
    public interface IRestService
    {
        /// <summary>
        /// Sisteme yeni server kaydı ekler.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> AddServerAsync(AddServerRequest req);

        /// <summary>
        /// Sisteme kayıtlı server kaydını siler.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> DeleteServerAsync(Guid ID);

        /// <summary>
        /// Sisteme yeni servis/uygulama kaydı ekler.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> AddApplicationAsync(AddServiceRequest req);

        /// <summary>
        /// Sisteme kayıtlı seris/uygulama kaydını siler.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> DeleteApplicationAsync(Guid ID);
    }
}
