using UnspedHealth.Core.DTOs;
using static UnspedHealth.Core.Enums;

namespace UnspedHealth.Core.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Uygulama içerisinden bildirim göndermek için kullanılır.
        /// </summary>
        /// <param name="notificationType">Bildirim Türü</param>
        /// <param name="request">Bildirim İçeriği</param>
        /// <returns>Taskın başarılı olup olmadığını döner.</returns>
        Task<bool> SendNotificationAsync(PushNotificationType notificationType, NotificationMessageRequest request);

        /// <summary>
        /// Aktif bildirim kanallarını işleyerek, her bir kanal için bildirim gönderir.
        /// </summary>
        /// <param name="activeChannels"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> ProcessActiveChannelsAsync(List<PushNotificationType> activeChannels, NotificationMessageRequest request);
        Task<bool> AddNotificationLogAsync(NotificationLogRequest req);
    }
}
