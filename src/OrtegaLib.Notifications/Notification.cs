namespace OrtegaLib.Notifications;

public abstract class Notification
{
    public string ServiceName { get; }

    public abstract string NotificationType { get; }

    public string Message { get; }

    protected Notification(
        string serviceName,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        ServiceName = serviceName;
        Message = message;
    }
}