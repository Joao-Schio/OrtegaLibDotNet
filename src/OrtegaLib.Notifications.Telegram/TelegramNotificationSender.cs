using System.Net.Http.Json;
using OrtegaLib.Models;

namespace OrtegaLib.Notifications.Telegram;

public sealed class TelegramNotificationSender(HttpClient client) : INotificationSender
{
    private readonly HttpClient _client = client;

    private async Task<NotificationError> Deserialize(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        var error = await content.ReadFromJsonAsync<NotificationError>(cancellationToken);
        if (error is null)
        {
            return NotificationError.Default();
        }

        return error;
    }

    public async Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        using var response = await _client.PostAsJsonAsync(
            "/notifications",
            notification,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await Deserialize(response.Content, cancellationToken);
            return Result<NotificationReceipt, NotificationError>.Failure(error);
        }

        var receipt = new NotificationReceipt(null, DateTimeOffset.Now);
        return Result<NotificationReceipt, NotificationError>.Success(receipt);
    }
}
