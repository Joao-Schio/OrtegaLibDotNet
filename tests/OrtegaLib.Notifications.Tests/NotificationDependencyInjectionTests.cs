using Microsoft.Extensions.DependencyInjection;
using OrtegaLib.Models;

namespace OrtegaLib.Notifications.Tests;

public sealed class NotificationDependencyInjectionTests
{
    [Fact]
    public void AddNotifications_RegistersCenterWithConfiguredServiceName()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INotificationSender, StubNotificationSender>();
        services.AddNotifications("ResumeSite");

        using var provider = services.BuildServiceProvider();

        var center = provider.GetRequiredService<NotificationCenter>();
        var notification = center.CreateIncomingMessage("hello");

        Assert.Equal("ResumeSite", notification.ServiceName);
        Assert.Equal("hello", notification.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddNotifications_ThrowsForBlankServiceName(string serviceName)
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(
            () => services.AddNotifications(serviceName));
    }

    private sealed class StubNotificationSender : INotificationSender
    {
        public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Result<NotificationReceipt, NotificationError>.Success(
                    new NotificationReceipt(null, DateTimeOffset.UtcNow)));
        }
    }
}
