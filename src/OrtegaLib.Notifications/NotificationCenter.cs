using OrtegaLib.Models;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications;

public sealed class NotificationCenter
{
    private readonly string _serviceName;
    private readonly INotificationSender _sender;

    public NotificationCenter(
        string serviceName,
        INotificationSender sender)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentNullException.ThrowIfNull(sender);

        _serviceName = serviceName;
        _sender = sender;
    }

    public ExceptionNotification CreateException(
        Exception exception,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new ExceptionNotification(
            _serviceName,
            message ?? exception.Message,
            exception);
    }

    public StatusNotification CreateStatus(
        string message,
        StatusNotification.Status status)
    {
        return new StatusNotification(
            _serviceName,
            message,
            status);
    }

    public IncomingMessageNotification CreateIncomingMessage(
        string message
    )
    {
        return new IncomingMessageNotification(
            _serviceName,
            message
        );
    }

    public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return _sender.SendAsync(
            notification,
            cancellationToken);
    }
}
