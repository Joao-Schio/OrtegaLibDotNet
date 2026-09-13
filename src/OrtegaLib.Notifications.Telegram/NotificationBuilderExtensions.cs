using OrtegaLib.Notifications;
using OrtegaLib.Notifications.Telegram;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelegramNotificationBuilderExtensions
{
    public static NotificationBuilder AddTelegram(
        this NotificationBuilder builder,
        Action<TelegramNotificationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new TelegramNotificationOptions();
        configure(options);

        var serviceUri = options.ServiceUri
            ?? throw new InvalidOperationException(
                "Telegram notification ServiceUri must be configured.");

        if (!serviceUri.IsAbsoluteUri)
        {
            throw new InvalidOperationException(
                "Telegram notification ServiceUri must be an absolute URI.");
        }

        builder.Services.AddHttpClient<INotificationSender, TelegramNotificationSender>(
            client => client.BaseAddress = serviceUri);

        return builder;
    }
}
