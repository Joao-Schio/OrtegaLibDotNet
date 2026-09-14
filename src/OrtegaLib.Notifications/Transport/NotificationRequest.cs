using System.Text.Json;

namespace OrtegaLib.Notifications;


public sealed class NotificationRequest
{
    public required string ServiceName { get; init; }

    public required string NotificationType { get; init; }

    public required string Message { get; init; }

    public JsonElement? Data { get; init; }

    public static NotificationRequest From(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return new NotificationRequest
        {
            ServiceName = notification.ServiceName,
            NotificationType = notification.NotificationType,
            Message = notification.Message,
            Data = notification.Data
        };
    }
}