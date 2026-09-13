namespace OrtegaLib.Notifications.Common;


public sealed class IncomingMessageNotification(string serviceName, string message) : Notification(serviceName, message)
{
    public override string NotificationType => "IncomingMessage";

}