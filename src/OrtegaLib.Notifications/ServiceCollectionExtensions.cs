using OrtegaLib.Notifications;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static NotificationBuilder AddNotifications(
        this IServiceCollection services,
        string serviceName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        services.AddTransient(sp =>
            new NotificationCenter(
                serviceName,
                sp.GetRequiredService<INotificationSender>()));

        return new NotificationBuilder(services);
    }
}
