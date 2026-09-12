using OrtegaLib.Models;

namespace OrtegaLib.Notifications;

public interface INotificationSender
{
    Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default);
}