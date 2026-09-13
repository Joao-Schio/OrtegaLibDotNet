using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrtegaLib.Models;


namespace OrtegaLib.Notifications.Telegram;



public class TelegramNotificationSender(HttpClient client)
{
    private readonly HttpClient _client = client;

    private async Task<NotificationError> Deserialize(HttpContent content, CancellationToken cancellationToken)
    {
        var error = await content.ReadFromJsonAsync<NotificationError>(cancellationToken);
        if (error is null)
        {
            return NotificationError.Default();
        }
        else
        {
            return error;
        }
    }

    public async Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.PostAsJsonAsync("/notifications", notification, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {      
            var error = await this.Deserialize(response.Content, cancellationToken);
            return Result<NotificationReceipt, NotificationError>.Failure(error);
        }
        else
        {
            var receipt = new NotificationReceipt(null, DateTimeOffset.Now);
            return Result<NotificationReceipt, NotificationError>.Success(receipt);
        }
    }
}
