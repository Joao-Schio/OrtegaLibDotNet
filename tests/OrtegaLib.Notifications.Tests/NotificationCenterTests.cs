using OrtegaLib.Models;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications.Tests;

public sealed class NotificationCenterTests
{
    [Fact]
    public void CreateException_UsesConfiguredServiceNameAndExceptionMessage()
    {
        var sender = new RecordingSender();
        var center = new NotificationCenter("test-service", sender);
        var exception = new InvalidOperationException("boom");

        var notification = center.CreateException(exception);

        Assert.Equal("test-service", notification.ServiceName);
        Assert.Equal("boom", notification.Message);
        Assert.Equal("exception", notification.NotificationType);
        Assert.True(notification.Data.HasValue);
    }

    [Fact]
    public void CreateException_UsesProvidedMessage()
    {
        var sender = new RecordingSender();
        var center = new NotificationCenter("test-service", sender);

        var notification = center.CreateException(
            new InvalidOperationException("boom"),
            "Unhandled exception");

        Assert.Equal("Unhandled exception", notification.Message);
    }

    [Fact]
    public void CreateStatus_UsesConfiguredServiceName()
    {
        var sender = new RecordingSender();
        var center = new NotificationCenter("test-service", sender);

        var notification = center.CreateStatus(
            "Service degraded",
            StatusNotification.Status.Degraded);

        Assert.Equal("test-service", notification.ServiceName);
        Assert.Equal("Service degraded", notification.Message);
        Assert.Equal("status", notification.NotificationType);
        Assert.Equal(
            StatusNotification.Status.Degraded,
            notification.CurrentStatus);
    }

    [Fact]
    public async Task SendAsync_ForwardsNotificationAndCancellationToken()
    {
        var sender = new RecordingSender();
        var center = new NotificationCenter("test-service", sender);
        var notification = center.CreateStatus(
            "Service healthy",
            StatusNotification.Status.Healthy);
        using var cancellation = new CancellationTokenSource();

        var result = await center.SendAsync(
            notification,
            cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Same(notification, sender.Notification);
        Assert.Equal(cancellation.Token, sender.CancellationToken);
    }

    [Fact]
    public void Constructor_Throws_WhenServiceNameIsBlank()
    {
        Assert.Throws<ArgumentException>(
            () => new NotificationCenter("", new RecordingSender()));
    }

    private sealed class RecordingSender : INotificationSender
    {
        public Notification? Notification { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            Notification = notification;
            CancellationToken = cancellationToken;

            return Task.FromResult(
                Result<NotificationReceipt, NotificationError>.Success(
                    new NotificationReceipt(
                        "test-id",
                        DateTimeOffset.UtcNow)));
        }
    }
}
