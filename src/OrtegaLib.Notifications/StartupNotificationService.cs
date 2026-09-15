using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications;

internal sealed class StartupNotificationService(
    IHostApplicationLifetime applicationLifetime,
    IServiceScopeFactory scopeFactory,
    ILogger<StartupNotificationService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var startupCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                applicationLifetime.ApplicationStarted,
                stoppingToken);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                startupCancellation.Token);
        }
        catch (OperationCanceledException)
            when (startupCancellation.IsCancellationRequested)
        {
        }

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();

            var notifications = scope.ServiceProvider
                .GetRequiredService<NotificationCenter>();

            var result = await notifications.SendAsync(
                notifications.CreateStatus(
                    StatusNotification.Status.Healthy),
                stoppingToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to send the startup notification: {ErrorCode} - {ErrorMessage}",
                    result.Error.Code,
                    result.Error.Message);
            }
        }
        catch (Exception exception)
            when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(
                exception,
                "Failed to send the startup notification.");
        }
    }
}
