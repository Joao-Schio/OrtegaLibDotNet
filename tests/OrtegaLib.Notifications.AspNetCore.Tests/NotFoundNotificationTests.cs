using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrtegaLib.Models;
using OrtegaLib.Notifications.Common;

namespace OrtegaLib.Notifications.AspNetCore.Tests;

public sealed class NotFoundNotificationTests
{
    [Fact]
    public async Task AddNotFoundNotification_SendsOriginalRequestUrlFor404()
    {
        var sender = new RecordingSender();

        await ExecuteRequestAsync(
            sender,
            async context =>
            {
                context.Request.Path = "/not-found";
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await Task.CompletedTask;
            });

        var notification = Assert.Single(sender.Notifications);
        var notFound = Assert.IsType<NotFoundNotification>(notification);

        Assert.Equal("Test service", notFound.ServiceName);
        Assert.Equal(
            "https://example.test/missing?source=test was requested",
            notFound.NotificationType);
    }

    [Fact]
    public async Task AddNotFoundNotification_DoesNotSendForSuccessfulResponse()
    {
        var sender = new RecordingSender();

        await ExecuteRequestAsync(
            sender,
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

        Assert.Empty(sender.Notifications);
    }

    [Fact]
    public async Task AddNotFoundNotification_DoesNotBreakRequestWhenSenderReturnsFailure()
    {
        var sender = new RecordingSender
        {
            Result = Result<NotificationReceipt, NotificationError>.Failure(
                new NotificationError(
                    "test_failure",
                    "The test sender failed.",
                    false))
        };

        var context = await ExecuteRequestAsync(
            sender,
            requestContext =>
            {
                requestContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            });

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Single(sender.Notifications);
    }

    [Fact]
    public async Task AddNotFoundNotification_DoesNotBreakRequestWhenSenderThrows()
    {
        var sender = new RecordingSender
        {
            ThrowOnSend = true
        };

        var context = await ExecuteRequestAsync(
            sender,
            requestContext =>
            {
                requestContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            });

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Single(sender.Notifications);
    }

    [Fact]
    public void AddNotFoundNotification_RegistersStartupFilterOnce()
    {
        var services = new ServiceCollection();

        services
            .AddNotifications("Test service")
            .AddNotFoundNotification()
            .AddNotFoundNotification();

        Assert.Single(
            services.Where(
                descriptor => descriptor.ServiceType == typeof(IStartupFilter)));
    }

    private static async Task<HttpContext> ExecuteRequestAsync(
        RecordingSender sender,
        RequestDelegate terminal)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<INotificationSender>(sender);

        services
            .AddNotifications("Test service")
            .AddNotFoundNotification();

        using var provider = services.BuildServiceProvider();

        var startupFilter = Assert.Single(
            provider.GetServices<IStartupFilter>());

        var appBuilder = new ApplicationBuilder(provider);

        startupFilter.Configure(
            app => app.Run(terminal))(appBuilder);

        var pipeline = appBuilder.Build();
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };

        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.test");
        context.Request.Path = "/missing";
        context.Request.QueryString = new QueryString("?source=test");

        await pipeline(context);

        return context;
    }

    private sealed class RecordingSender : INotificationSender
    {
        public List<Notification> Notifications { get; } = [];

        public Result<NotificationReceipt, NotificationError> Result { get; init; } =
            Result<NotificationReceipt, NotificationError>.Success(
                new NotificationReceipt(
                    null,
                    DateTimeOffset.UtcNow));

        public bool ThrowOnSend { get; init; }

        public Task<Result<NotificationReceipt, NotificationError>> SendAsync(
            Notification notification,
            CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification);

            if (ThrowOnSend)
            {
                throw new InvalidOperationException("The test sender threw.");
            }

            return Task.FromResult(Result);
        }
    }
}
