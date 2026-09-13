using Microsoft.Extensions.DependencyInjection;

namespace OrtegaLib.Notifications;

public sealed class NotificationBuilder
{
    internal NotificationBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }

    public IServiceCollection Services { get; }
}
