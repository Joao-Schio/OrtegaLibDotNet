using System.Text.Json;

namespace OrtegaLib.Notifications;

public abstract class Notification
{
    public string ServiceName { get; }

    public abstract string NotificationType { get; }

    public string Message { get; }

    public JsonElement? Data { get; } 

    protected Notification(
        string serviceName,
        string message,
        JsonElement? data = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        ServiceName = serviceName;
        Message = message;
        Data = data;
    }
}