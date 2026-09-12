namespace OrtegaLib.Notifications.Common;

public sealed class ExceptionNotification : Notification
{
    public sealed record ExceptionDetails(
        string Type,
        string Message,
        string? StackTrace,
        ExceptionDetails? InnerException = null);

    public override string NotificationType => "exception";

    public ExceptionDetails Exception { get; }

    public ExceptionNotification(
        string serviceName,
        string message,
        Exception exception)
        : base(serviceName, message)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Exception = FromException(exception);
    }

    private static ExceptionDetails FromException(Exception exception)
    {
        return new ExceptionDetails(
            Type: exception.GetType().FullName
                ?? exception.GetType().Name,
            Message: exception.Message,
            StackTrace: exception.StackTrace,
            InnerException: exception.InnerException is null
                ? null
                : FromException(exception.InnerException));
    }
}