namespace OrtegaLib.Notifications.Common;

public sealed class StatusNotification(
    string serviceName,
    string message,
    StatusNotification.Status status)
    : Notification(serviceName, message)
{
    public enum Status
    {
        Healthy,
        Degraded,
        Unhealthy
    }

    public override string NotificationType => "status";

    public Status CurrentStatus { get; } = status;
}