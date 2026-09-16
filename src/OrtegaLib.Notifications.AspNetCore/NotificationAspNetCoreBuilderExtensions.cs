using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrtegaLib.Notifications;
using OrtegaLib.Notifications.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class NotificationAspNetCoreBuilderExtensions
{
    public static NotificationBuilder AddNotFoundNotification(
        this NotificationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IStartupFilter, NotFoundNotificationStartupFilter>());

        return builder;
    }
}
