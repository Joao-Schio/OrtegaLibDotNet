namespace OrtegaLib.Notifications;


public sealed record NotificationError(
    string Code,
    string Message,
    bool IsTransient);