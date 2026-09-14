using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OrtegaLib.Notifications.Telegram.Transport;
using OrtegaLib.Notifications.Transport;

namespace OrtegaLib.Notifications.Telegram.Tests;

public sealed class HttpNotificationTransportTests
{
    [Fact]
    public async Task SendAsync_PostsRequestAndReturnsReceipt_OnSuccess()
    {
        var handler = new RecordingHandler(
            _ => new HttpResponseMessage(HttpStatusCode.NoContent));

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://notifications.test")
        };

        var transport = new HttpNotificationTransport(client);
        var request = CreateRequest();

        var beforeSend = DateTimeOffset.Now;
        var result = await transport.SendAsync(request);
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
            request.ServiceName,
            root.GetProperty("serviceName").GetString());
        Assert.Equal(
            request.NotificationType,
            root.GetProperty("notificationType").GetString());
        Assert.Equal(
            request.Message,
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

        var transport = new HttpNotificationTransport(client);

        var result = await transport.SendAsync(CreateRequest());

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

        var transport = new HttpNotificationTransport(client);

        var result = await transport.SendAsync(CreateRequest());

        Assert.True(result.IsFailure);
        Assert.Equal("Null error response", result.Error.Code);
        Assert.Equal("The json converter returned null", result.Error.Message);
        Assert.False(result.Error.IsTransient);
    }

    private static NotificationRequest CreateRequest()
    {
        return new NotificationRequest
        {
            ServiceName = "test-service",
            NotificationType = "exception",
            Message = "Unhandled exception",
            Data = JsonSerializer.SerializeToElement(new
            {
                type = "System.InvalidOperationException",
                message = "boom"
            })
        };
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
}
