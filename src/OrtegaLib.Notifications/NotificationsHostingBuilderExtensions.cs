using OrtegaLib.Notifications;

namespace Microsoft.Extensions.DependencyInjection;

public static class NotificationHostingBuilderExtensions
{
    public static NotificationBuilder AddStartupNotification(
        this NotificationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHostedService<StartupNotificationService>();

        return builder;
    }
}