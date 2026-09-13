namespace OrtegaLib.Notifications.Common;


public class NotFoundNotification(string serviceName, string endpoint) : Notification(serviceName, "An unexisting endpoint was requested")
{
    public override string NotificationType => $"{endpoint} was requested";
}