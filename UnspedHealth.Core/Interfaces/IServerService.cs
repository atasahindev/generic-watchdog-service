using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Entities;

namespace UnspedHealth.Core.Interfaces
{
    public interface IServerService
    {
        /// <summary>
        /// Sistemde kayıtlı olan tüm aktif sunucuları pingler, durumlarını veritabanında günceller 
        /// ve erişilemeyen sunucuların listesini döner.
        /// </summary>
        /// <returns>Erişilemeyen (Status = 0) sunucuların listesi.</returns>
        Task<List<Server>> CheckAllServersAsync();

        /// <summary>
        /// Sisteme yeni server kaydı ekler.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> AddAsync(AddServerRequest req);

        /// <summary>
        /// Sisteme kayıtlı server kaydını siler.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(Guid ID);

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
