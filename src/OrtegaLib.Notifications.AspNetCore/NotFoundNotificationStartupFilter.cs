using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace OrtegaLib.Notifications.AspNetCore;

internal sealed class NotFoundNotificationStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(
        Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            app.UseMiddleware<NotFoundNotificationMiddleware>();
            next(app);
        };
    }
}
