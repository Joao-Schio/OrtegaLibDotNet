using OrtegaLib.Models;
using OrtegaLib.Notifications.Transport;

namespace OrtegaLib.Notifications.Telegram.Transport;

internal interface INotificationTransport
{
    Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default);
}
