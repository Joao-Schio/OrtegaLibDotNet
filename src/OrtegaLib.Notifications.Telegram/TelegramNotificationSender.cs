using OrtegaLib.Models;
using OrtegaLib.Notifications.Telegram.Transport;
using OrtegaLib.Notifications.Transport;

namespace OrtegaLib.Notifications.Telegram;

internal sealed class TelegramNotificationSender(
    INotificationTransport transport) : INotificationSender
{
    public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var request = NotificationRequest.From(notification);

        return transport.SendAsync(request, cancellationToken);
    }
}
