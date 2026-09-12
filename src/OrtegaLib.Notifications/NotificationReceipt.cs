namespace OrtegaLib.Notifications;


public sealed record NotificationReceipt (
    string? ExternalId,
    DateTimeOffset SentAt
);