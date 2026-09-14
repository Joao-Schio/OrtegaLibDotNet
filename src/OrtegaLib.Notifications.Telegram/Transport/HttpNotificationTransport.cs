using System.Net.Http.Json;
using OrtegaLib.Models;
using OrtegaLib.Notifications.Transport;

namespace OrtegaLib.Notifications.Telegram.Transport;

internal sealed class HttpNotificationTransport(HttpClient client)
    : INotificationTransport
{
    private readonly HttpClient _client = client;

    public async Task<Result<NotificationReceipt, NotificationError>> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _client.PostAsJsonAsync(
            "/notifications",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await DeserializeErrorAsync(
                response.Content,
                cancellationToken);

            return Result<NotificationReceipt, NotificationError>.Failure(error);
        }

        var receipt = new NotificationReceipt(null, DateTimeOffset.Now);
        return Result<NotificationReceipt, NotificationError>.Success(receipt);
    }

    private static async Task<NotificationError> DeserializeErrorAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        var error = await content.ReadFromJsonAsync<NotificationError>(
            cancellationToken);

        return error ?? NotificationError.Default();
    }
}
