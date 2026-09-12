namespace OrtegaLib.Notifications.Generic;



public sealed class StartupNotification(
    string serviceName,
    string message) : Notification(serviceName, message)
{
    public override string NotificationType => "startup";
}