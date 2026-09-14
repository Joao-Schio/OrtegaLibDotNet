using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OrtegaLib.Notifications;
using OrtegaLib.Notifications.Common;
using OrtegaLib.Notifications.Transport;
using OrtegaLib.Notifications.Telegram;
using OrtegaLib.Models;


namespace OrtegaLib.Notifications.Telegram.Tests;

public sealed class TelegramNotificationSenderTests
{
    [Fact]
    public async Task SendAsync_PostsNotificationAndReturnsReceipt_OnSuccess()
    {
        var handler = new RecordingHandler(
            _ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://notifications.test")
        };

        var sender = new TelegramNotificationSender(client);
        var notification = new ExceptionNotification(
            "test-service",
            "Unhandled exception",
            new InvalidOperationException("boom"));

        var beforeSend = DateTimeOffset.Now;
        var result = await sender.SendAsync(notification);
        var afterSend = DateTimeOffset.Now;

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ExternalId);
        Assert.InRange(result.Value.SentAt, beforeSend, afterSend);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(
            new Uri("https://notifications.test/notifications"),
            handler.RequestUri);
        Assert.Equal("application/json", handler.ContentType);
        Assert.NotNull(handler.Body);

        using var payload = JsonDocument.Parse(handler.Body);
        var root = payload.RootElement;

        Assert.Equal(
            "test-service",
            root.GetProperty("serviceName").GetString());
        Assert.Equal(
            "exception",
            root.GetProperty("notificationType").GetString());
        Assert.Equal(
            "Unhandled exception",
            root.GetProperty("message").GetString());
        Assert.Equal(
            JsonValueKind.Object,
            root.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task SendAsync_ReturnsServerError_OnFailureResponse()
    {
        var expectedError = new NotificationError(
            "rate_limited",
            "Try again later",
            true);

        var handler = new RecordingHandler(
            _ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = JsonContent.Create(expectedError)
            });

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://notifications.test")
        };

        var sender = new TelegramNotificationSender(client);
        var notification = new StatusNotification(
            "test-service",
            "Service healthy",
            StatusNotification.Status.Healthy);

        var result = await sender.SendAsync(notification);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.Error.Code);
        Assert.Equal(expectedError.Message, result.Error.Message);
        Assert.Equal(expectedError.IsTransient, result.Error.IsTransient);
    }

    [Fact]
    public async Task SendAsync_ReturnsDefaultError_WhenFailureBodyIsNull()
    {
        var handler = new RecordingHandler(
            _ => new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent(
                    "null",
                    Encoding.UTF8,
                    "application/json")
            });

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://notifications.test")
        };

        var sender = new TelegramNotificationSender(client);
        var notification = new StatusNotification(
            "test-service",
            "Service unhealthy",
            StatusNotification.Status.Unhealthy);

        var result = await sender.SendAsync(notification);

        Assert.True(result.IsFailure);
        Assert.Equal("Null error response", result.Error.Code);
        Assert.Equal("The json converter returned null", result.Error.Message);
        Assert.False(result.Error.IsTransient);
    }

    [Fact]
    public async Task SendAsync_ConvertsNotificationAndDelegatesToTransport()
    {
        var transport = new FakeNotificationTransport();
        var sender = new TelegramNotificationSender(transport);

        var notification = new StatusNotification(
            "resumesite",
            "Service started",
            StatusNotification.Status.Healthy);

        await sender.SendAsync(notification);

        Assert.NotNull(transport.Request);
        Assert.Equal("resumesite", transport.Request.ServiceName);
        Assert.Equal("status", transport.Request.NotificationType);
        Assert.Equal("Service started", transport.Request.Message);
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? ContentType { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return responseFactory(request);
        }
    }


    private sealed class FakeNotificationTransport : INotificationTransport
    {
        public NotificationRequest? Request { get; private set; }
    
        public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
            NotificationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
    
            return Task.FromResult(
                Result<NotificationReceipt, NotificationError>.Success(
                    new NotificationReceipt(null, DateTimeOffset.UtcNow)));
        }
    }
}
