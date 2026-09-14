using System.Text.Json;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications.Tests;

public sealed class NotificationRequestTests
{
    private static readonly JsonSerializerOptions WebJsonOptions =
        new(JsonSerializerDefaults.Web);

    [Fact]
    public void From_CopiesNotificationContract()
    {
        var notification = new ExceptionNotification(
            "resumesite",
            "Unhandled exception",
            new InvalidOperationException("boom"));

        var request = NotificationRequest.From(notification);

        Assert.Equal(notification.ServiceName, request.ServiceName);
        Assert.Equal(notification.NotificationType, request.NotificationType);
        Assert.Equal(notification.Message, request.Message);
        Assert.Equal(notification.Data, request.Data);
    }

    [Fact]
    public void From_Throws_WhenNotificationIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => NotificationRequest.From(null!));
    }

    [Fact]
    public void Serialize_UsesExpectedWebContract()
    {
        var request = new NotificationRequest
        {
            ServiceName = "resumesite",
            NotificationType = "startup",
            Message = "some random message",
            Data = null
        };

        var json = JsonSerializer.Serialize(request, WebJsonOptions);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("resumesite", root.GetProperty("serviceName").GetString());
        Assert.Equal("startup", root.GetProperty("notificationType").GetString());
        Assert.Equal("some random message", root.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    [Fact]
    public void Deserialize_RoundTripsTransportPayload()
    {
        const string json = """
            {
              "serviceName": "resumesite",
              "notificationType": "exception",
              "message": "Unhandled exception",
              "data": {
                "type": "System.InvalidOperationException",
                "message": "boom"
              }
            }
            """;

        var request = JsonSerializer.Deserialize<NotificationRequest>(
            json,
            WebJsonOptions);

        Assert.NotNull(request);
        Assert.Equal("resumesite", request.ServiceName);
        Assert.Equal("exception", request.NotificationType);
        Assert.Equal("Unhandled exception", request.Message);
        Assert.True(request.Data.HasValue);
        Assert.Equal(
            "System.InvalidOperationException",
            request.Data.Value.GetProperty("type").GetString());
        Assert.Equal(
            "boom",
            request.Data.Value.GetProperty("message").GetString());
    }
}
