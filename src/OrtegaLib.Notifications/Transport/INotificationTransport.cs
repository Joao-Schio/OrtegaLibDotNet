using OrtegaLib.Models;

namespace OrtegaLib.Notifications.Transport;

public interface INotificationTransport
{
    Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default);
}