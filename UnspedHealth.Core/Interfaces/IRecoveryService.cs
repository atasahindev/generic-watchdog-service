using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Interfaces
{
    public interface IRecoveryService
    {
        /// <summary>
        /// Down olan windows servisi yeniden başlatmak için uygulamanın çalıştığı makineye uzaktan bağlanır ve servisi yeniden başlatmak için kullanılır.
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="serviceName"></param>
        /// <param name="executionPath"></param>
        /// <returns>İşlem sonucunu ve hata durumunda alınan mesajı döner (bool, string)</returns>
        public Task<(bool IsSuccess, string Message)> RestartWindowsServiceAsync(string serverIP, string serviceName, string executionPath);

        /// <summary>
        /// Lokal makinede down olan scheduled taski veya execute edilebilir herhangi bir app'i yeniden başlatmak için uygulamanın çalıştığı makineye uzaktan bağlanır ve exeyi yeniden başlatmak için kullanılır.
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="serviceName"></param>
        /// <param name="executionPath"></param>
        /// <returns>İşlem sonucunu ve hata durumunda alınan mesajı döner (bool, string)</returns>
        public Task<(bool IsSuccess, string Message)> RestartLocalProcessAsync(string processName, string executionPath);

        /// <summary>
        /// Uzak makinede down olan scheduled taski veya execute edilebilir herhangi bir app'i yeniden başlatmak için uygulamanın çalıştığı makineye uzaktan bağlanır ve exeyi yeniden başlatmak için kullanılır.
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="serviceName"></param>
        /// <param name="executionPath"></param>
        /// <returns>İşlem sonucunu ve hata durumunda alınan mesajı döner (bool, string)</returns>
        public Task<(bool IsSuccess, string Message)> RestartRemoteProcessAsync(string serverIP, string processName, string executionPath);
    }
}
