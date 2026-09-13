# OrtegaLib

Small, explicit .NET libraries for shared application models and notification contracts.

> **Status:** early preview. The public API may still change before `1.0.0`.

OrtegaLib is intentionally split into small NuGet packages so applications can depend only on the pieces they actually need.

## Packages

| Package | Purpose | OrtegaLib dependencies |
| --- | --- | --- |
| [`OrtegaLib.Models`](https://www.nuget.org/packages/OrtegaLib.Models/) | Small shared model primitives such as `Result<T, E>`. | None |
| [`OrtegaLib.Notifications`](https://www.nuget.org/packages/OrtegaLib.Notifications/) | Transport-agnostic notification contracts, `NotificationCenter`, and common notification types. | `OrtegaLib.Models` |
| [`OrtegaLib.Notifications.Telegram`](https://www.nuget.org/packages/OrtegaLib.Notifications.Telegram/) | HTTP transport for sending OrtegaLib notifications to a separate Telegram client/notification service. | `OrtegaLib.Notifications`, `OrtegaLib.Models` |

All packages currently target **.NET 10**.

## Installation

Install only the package you need.

### Models only

```bash
dotnet add package OrtegaLib.Models --version 0.1.0-preview.1
```

### Notification contracts

```bash
dotnet add package OrtegaLib.Notifications --version 0.1.0-preview.1
```

### Telegram transport

```bash
dotnet add package OrtegaLib.Notifications.Telegram --version 0.1.0-preview.1
```

NuGet resolves the lower-level OrtegaLib dependencies transitively.

## `OrtegaLib.Models`

The models package is deliberately independent from the notification packages.

For example, `Result<T, E>` represents an operation that succeeds with a value or fails with an error:

```csharp
using OrtegaLib.Models;

Result<int, string> Parse(string input)
{
    if (int.TryParse(input, out var value))
    {
        return Result<int, string>.Success(value);
    }

    return Result<int, string>.Failure("Invalid integer.");
}
```

Consumers that only need shared models can reference `OrtegaLib.Models` without pulling in notification or HTTP infrastructure.

## Notifications

`OrtegaLib.Notifications` separates notification creation from delivery.

The central contract is `INotificationSender`, while `NotificationCenter` provides the application-facing API and stores the service name once.

Built-in common notifications currently include:

- exception notifications
- status notifications
- incoming-message notifications
- not-found notifications

### Dependency injection

Register notifications with the application's service name:

```csharp
builder.Services.AddNotifications("ResumeSite");
```

A transport package then provides the `INotificationSender` implementation.

## Telegram transport

`OrtegaLib.Notifications.Telegram` does **not** communicate directly with the Telegram Bot API.

It is an HTTP adapter intended to communicate with a separate Telegram client/notification service. That service owns Telegram-specific concerns such as bot credentials, destination identity, rendering, and delivery.

Configure it during startup:

```csharp
builder.Services
    .AddNotifications("ResumeSite")
    .AddTelegram(options =>
    {
        options.ServiceUri = new Uri(
            builder.Configuration["TelegramClient:Uri"]!);
    });
```

Then inject `NotificationCenter` where notifications are needed:

```csharp
using OrtegaLib.Notifications;

public sealed class ContactService(NotificationCenter notifications)
{
    public async Task NotifyAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var notification = notifications.CreateIncomingMessage(message);

        await notifications.SendAsync(
            notification,
            cancellationToken);
    }
}
```

Exception notifications follow the same path:

```csharp
try
{
    await DoWorkAsync();
}
catch (Exception exception)
{
    await notifications.SendAsync(
        notifications.CreateException(exception));
}
```

## Design goals

OrtegaLib favors small APIs and explicit contracts over framework-heavy abstractions.

The main goals are:

- keep shared models independently usable
- keep notification contracts independent from transports
- keep application identity/configuration at startup instead of scattered through callers
- keep transport-specific concerns in transport packages
- make invalid or ambiguous states harder to express where practical
- avoid forcing applications to depend on functionality they do not use

The intended dependency direction is:

```text
Application
    ↓
NotificationCenter
    ↓
INotificationSender
    ↑
Transport package
```

and at the package level:

```text
OrtegaLib.Models
        ↑
OrtegaLib.Notifications
        ↑
OrtegaLib.Notifications.Telegram
```

## Building from source

```bash
dotnet restore OrtegaLibDotNet.sln
dotnet build OrtegaLibDotNet.sln
dotnet test OrtegaLibDotNet.sln
```

## Versioning

The project is currently published as preview packages. Until `1.0.0`, breaking API changes may occur between preview releases.
