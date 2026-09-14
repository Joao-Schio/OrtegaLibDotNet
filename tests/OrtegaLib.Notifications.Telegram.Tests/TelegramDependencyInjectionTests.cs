using Microsoft.Extensions.DependencyInjection;
using OrtegaLib.Notifications.Transport;
namespace OrtegaLib.Notifications.Telegram.Tests;

public sealed class TelegramDependencyInjectionTests
{
    [Fact]
    public void AddTelegram_RegistersSenderAndNotificationCenter()
    {
        var services = new ServiceCollection();

        services
            .AddNotifications("ResumeSite")
            .AddTelegram(options =>
            {
                options.ServiceUri = new Uri("https://notifications.test");
            });

        using var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredService<INotificationSender>();
        var center = provider.GetRequiredService<NotificationCenter>();
        var notification = center.CreateIncomingMessage("hello");

        Assert.IsType<TelegramNotificationSender>(sender);
        Assert.Equal("ResumeSite", notification.ServiceName);
    }

    [Fact]
    public void AddTelegram_ThrowsWhenServiceUriIsMissing()
    {
        var services = new ServiceCollection();
        var builder = services.AddNotifications("ResumeSite");

        var exception = Assert.Throws<InvalidOperationException>(
            () => builder.AddTelegram(_ => { }));

        Assert.Contains("ServiceUri", exception.Message);
    }

    [Fact]
    public void AddTelegram_ThrowsWhenServiceUriIsRelative()
    {
        var services = new ServiceCollection();
        var builder = services.AddNotifications("ResumeSite");

        var exception = Assert.Throws<InvalidOperationException>(
            () => builder.AddTelegram(options =>
            {
                options.ServiceUri = new Uri("notifications", UriKind.Relative);
            }));

        Assert.Contains("absolute URI", exception.Message);
    }
}
