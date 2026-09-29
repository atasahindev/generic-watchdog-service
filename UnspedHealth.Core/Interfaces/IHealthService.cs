using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.Core.DTOs;
using UnspedHealth.Core.Entities;

namespace UnspedHealth.Core.Interfaces
{
    public interface IHealthService
    {
        /// <summary>
        /// ID'si verilen servisin heartbeat sinyalini işler.
        /// </summary>
        /// <returns>Sinyalin başarıyla alınıp alınmadığını dönen boolean değer.</returns>
        public Task<bool> ReceiveHeartbeatAsync(Guid serviceID);

        /// <summary>
        /// ID'si verilen servisin heartbeat sinyalini işler, ek bilgilerle birlikte gönderilir. Exception durumları için kullanılır. Degraded (bozuk) olarak işaretlenebilir.
        /// </summary>
        /// <returns>Sinyalin başarıyla alınıp alınmadığını dönen boolean değer.</returns>
        public Task<bool> ReceiveHeartbeatAsync(HeartbeatRequest req);

        /// <summary>
        /// Sistemdeki tüm servislerin sağlık durumlarını kontrol eder, belirli aralıklarla sinyal göndermeyen servisleri "Down" olarak işaretler. (WINDOWS SERVICE, NET API VS.)
        /// </summary>
        /// <param name="workerIntervalInSeconds"></param>
        /// <returns>Down olan servisleri döner.</returns>
        public Task<List<ServiceHealthCheck>> CheckAllServicesAsync(int workerIntervalInSeconds);

        /// <summary>
        /// Sistemdeki tüm dış python ve AI servislerinin durumlarını servislerin kendi healthcheck endpointlerine istek göndererek kontrol eder. (PYTHON, DOCKER, AI HOST VS.)
        public Task<List<ServiceHealthCheck>> CheckAllExternalPythonServicesAsync();

        /// <summary>
        /// Belirli bir servisin o anki sağlık durumunu anlık olarak kontrol eder.
        /// </summary>
        /// <param name="serviceId">Kontrol edilecek servisin Unique Identifier'ı.</param>
        /// <returns>Servisin online olup olmadığını ve detaylarını döner.</returns>
        Task<(bool IsOnline, string Message)> GetSingleServiceStatusAsync(Guid serviceId);

    }
}
