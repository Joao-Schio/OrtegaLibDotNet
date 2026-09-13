namespace OrtegaLib.Notifications.Telegram;


public sealed class TelegramNotificationOptions(Uri serviceUri)
{
    public Uri ServiceUri { get; init; } = serviceUri;
}