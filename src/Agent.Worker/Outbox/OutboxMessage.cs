namespace Agent.Worker.Outbox;

/// <summary>
/// רשומת Outbox מתמשכת (SQLite). האירוע נשמר כאן *לפני* השליחה לשרת,
/// כך שגם אם השרת/DB נפולים — האירוע לא נאבד וישודר מחדש (§4, §9 בתוכנית).
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>מפתח idempotency שנשלח לשרת למניעת כפילות בשידור חוזר.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>גוף הבקשה (IngestEventRequest) כ-JSON מוכן לשליחה.</summary>
    public string PayloadJson { get; set; } = string.Empty;

    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum OutboxStatus
{
    Pending = 0,
    Sent = 1
}
