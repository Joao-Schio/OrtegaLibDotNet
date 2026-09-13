namespace OrtegaLib.Notifications;


public sealed class NotificationError(string code,string message,bool isTransient)
{
    public string Code { get; init; } = code;
    public string Message { get; init; } = message;
    public bool IsTransient { get; init; } = isTransient;


    public static NotificationError Default()
    {
        return new NotificationError(
            "Null error response",
            "The json converter returned null",
            false
        );
    }
}