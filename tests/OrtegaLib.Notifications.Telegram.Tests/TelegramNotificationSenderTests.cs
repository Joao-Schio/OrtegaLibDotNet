using OrtegaLib.Models;
using OrtegaLib.Notifications.Common;
using OrtegaLib.Notifications.Telegram.Transport;
using OrtegaLib.Notifications.Transport;

namespace OrtegaLib.Notifications.Telegram.Tests;

public sealed class TelegramNotificationSenderTests
{
    [Fact]
    public async Task SendAsync_ConvertsNotificationAndDelegatesToTransport()
    {
        using var cancellationSource = new CancellationTokenSource();
        var transport = new FakeNotificationTransport();
        var sender = new TelegramNotificationSender(transport);

        var notification = new StatusNotification(
            "resumesite",
            "Service started",
            StatusNotification.Status.Healthy);

        await sender.SendAsync(
            notification,
            cancellationSource.Token);

        Assert.NotNull(transport.Request);
        Assert.Equal(notification.ServiceName, transport.Request.ServiceName);
        Assert.Equal(notification.NotificationType, transport.Request.NotificationType);
        Assert.Equal(notification.Message, transport.Request.Message);
        Assert.Equal(notification.Data, transport.Request.Data);
        Assert.Equal(cancellationSource.Token, transport.CancellationToken);
    }

    [Fact]
    public async Task SendAsync_ReturnsTransportResult()
    {
        var expectedError = new NotificationError(
            "transport_failure",
            "Transport failed",
            true);

        var expectedResult =
            Result<NotificationReceipt, NotificationError>.Failure(expectedError);

        var transport = new FakeNotificationTransport(expectedResult);
        var sender = new TelegramNotificationSender(transport);
        var notification = new IncomingMessageNotification(
            "resumesite",
            "hello");

        var result = await sender.SendAsync(notification);

        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenNotificationIsNull()
    {
        var sender = new TelegramNotificationSender(
            new FakeNotificationTransport());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sender.SendAsync(null!));
    }

    private sealed class FakeNotificationTransport(
        Result<NotificationReceipt, NotificationError>? result = null)
        : INotificationTransport
    {
        private readonly Result<NotificationReceipt, NotificationError> _result =
            result ?? Result<NotificationReceipt, NotificationError>.Success(
                new NotificationReceipt(null, DateTimeOffset.UtcNow));

        public NotificationRequest? Request { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
            NotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            CancellationToken = cancellationToken;

            return Task.FromResult(_result);
        }
    }
}
