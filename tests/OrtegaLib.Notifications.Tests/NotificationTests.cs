using System.Text.Json;
using OrtegaLib.Notifications;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications.Tests;

public sealed class NotificationTests
{
    [Fact]
    public void Constructor_Throws_WhenServiceNameIsBlank()
    {
        Assert.Throws<ArgumentException>(
            () => new TestNotification("", "message"));
    }

    [Fact]
    public void Constructor_Throws_WhenMessageIsBlank()
    {
        Assert.Throws<ArgumentException>(
            () => new TestNotification("service", ""));
    }

    [Fact]
    public void ExceptionNotification_SerializesExceptionIntoData()
    {
        var innerException = new ArgumentException("inner failure");
        var exception = new InvalidOperationException(
            "outer failure",
            innerException);

        var notification = new ExceptionNotification(
            "test-service",
            "Unhandled exception",
            exception);

        Assert.Equal("test-service", notification.ServiceName);
        Assert.Equal("Unhandled exception", notification.Message);
        Assert.Equal("exception", notification.NotificationType);
        Assert.True(notification.Data.HasValue);

        var data = notification.Data.Value;

        Assert.Equal(
            typeof(InvalidOperationException).FullName,
            data.GetProperty("Type").GetString());
        Assert.Equal(
            "outer failure",
            data.GetProperty("Message").GetString());

        var inner = data.GetProperty("InnerException");

        Assert.Equal(
            typeof(ArgumentException).FullName,
            inner.GetProperty("Type").GetString());
        Assert.Equal(
            "inner failure",
            inner.GetProperty("Message").GetString());
        Assert.Equal(
            JsonValueKind.Null,
            inner.GetProperty("InnerException").ValueKind);
    }

    [Fact]
    public void ExceptionNotification_Throws_WhenExceptionIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ExceptionNotification(
                "test-service",
                "Unhandled exception",
                null!));
    }

    [Fact]
    public void StatusNotification_ExposesStatusWithoutAdditionalData()
    {
        var notification = new StatusNotification(
            "test-service",
            "Service degraded",
            StatusNotification.Status.Degraded);

        Assert.Equal("test-service", notification.ServiceName);
        Assert.Equal("Service degraded", notification.Message);
        Assert.Equal("status", notification.NotificationType);
        Assert.Equal(
            StatusNotification.Status.Degraded,
            notification.CurrentStatus);
        Assert.False(notification.Data.HasValue);
    }

    private sealed class TestNotification(
        string serviceName,
        string message,
        JsonElement? data = null)
        : Notification(serviceName, message, data)
    {
        public override string NotificationType => "test";
    }
}
