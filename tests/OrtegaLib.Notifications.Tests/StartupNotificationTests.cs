using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrtegaLib.Models;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications.Tests;

public sealed class StartupNotificationTests
{
    [Fact]
    public async Task StartupNotification_WaitsForApplicationStarted()
    {
        using var lifetime = new TestApplicationLifetime();
        var sender = new RecordingSender();

        using var provider = CreateProvider(lifetime, sender);

        var service = Assert.Single(
            provider.GetServices<IHostedService>());

        await service.StartAsync(CancellationToken.None);

        Assert.False(sender.NotificationSent.IsCompleted);

        lifetime.SignalStarted();

        var notification = await sender.NotificationSent;

        var status = Assert.IsType<StatusNotification>(notification);

        Assert.Equal(
            StatusNotification.Status.Healthy,
            status.CurrentStatus);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartupNotification_DoesNotSendWhenHostStopsBeforeStartup()
    {
        using var lifetime = new TestApplicationLifetime();
        var sender = new RecordingSender();

        using var provider = CreateProvider(lifetime, sender);

        var service = Assert.Single(
            provider.GetServices<IHostedService>());

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        lifetime.SignalStarted();

        Assert.False(sender.NotificationSent.IsCompleted);
    }

    private static ServiceProvider CreateProvider(
        TestApplicationLifetime lifetime,
        RecordingSender sender)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IHostApplicationLifetime>(lifetime);
        services.AddSingleton<INotificationSender>(sender);
        services.AddSingleton(
            typeof(ILogger<>),
            typeof(TestLogger<>));

        services
            .AddNotifications("Test service")
            .AddStartupNotification();

        return services.BuildServiceProvider();
    }

    private sealed class RecordingSender : INotificationSender
    {
        private readonly TaskCompletionSource<Notification> _notificationSent =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<Notification> NotificationSent =>
            _notificationSent.Task;

        public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            _notificationSent.TrySetResult(notification);

            return Task.FromResult(
                Result<NotificationReceipt, NotificationError>.Success(
                    new NotificationReceipt(
                        null,
                        DateTimeOffset.UtcNow)));
        }
    }

    private sealed class TestApplicationLifetime :
        IHostApplicationLifetime,
        IDisposable
    {
        private readonly CancellationTokenSource _started = new();
        private readonly CancellationTokenSource _stopping = new();
        private readonly CancellationTokenSource _stopped = new();

        public CancellationToken ApplicationStarted => _started.Token;

        public CancellationToken ApplicationStopping => _stopping.Token;

        public CancellationToken ApplicationStopped => _stopped.Token;

        public void SignalStarted() => _started.Cancel();

        public void StopApplication() => _stopping.Cancel();

        public void Dispose()
        {
            _started.Dispose();
            _stopping.Dispose();
            _stopped.Dispose();
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}