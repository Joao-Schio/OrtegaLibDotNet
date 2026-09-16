using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace OrtegaLib.Notifications.AspNetCore;

internal sealed class NotFoundNotificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NotFoundNotificationMiddleware> _logger;

    public NotFoundNotificationMiddleware(
        RequestDelegate next,
        ILogger<NotFoundNotificationMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        NotificationCenter notifications)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(notifications);

        var endpoint = context.Request.GetDisplayUrl();

        await _next(context);

        if (context.Response.StatusCode != StatusCodes.Status404NotFound)
        {
            return;
        }

        try
        {
            var result = await notifications.SendAsync(
                notifications.CreateNotFound(endpoint),
                CancellationToken.None);

            if (result.IsFailure)
            {
                _logger.LogError(
                    "Failed to send the not-found notification for {Endpoint}: {ErrorCode} - {ErrorMessage}",
                    endpoint,
                    result.Error.Code,
                    result.Error.Message);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to send the not-found notification for {Endpoint}.",
                endpoint);
        }
    }
}
