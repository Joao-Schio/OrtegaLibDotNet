namespace OrtegaLib.Notifications.Common;


public sealed class ExceptionNotification(
    string serviceName,
    string message,
    Exception exception) : Notification(serviceName, message)
{
    public override string NotificationType => "exception";

    public Exception Exception { get; } = exception;
}