namespace OrtegaLib.Notifications.Common;



public sealed class StartupNotification(
    string serviceName,
    string message) : Notification(serviceName, message)
{
    public override string NotificationType => "startup";
}