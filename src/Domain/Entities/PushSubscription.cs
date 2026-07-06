namespace Domain.Entities;

/// <summary>
/// מנוי Web Push של משתמש (endpoint + מפתחות). משמש לשליחת התראה כשהדפדפן סגור.
/// נשמר בעת הרשמת הלקוח ל-Push (ראה §5 בתוכנית).
/// </summary>
public class PushSubscription
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string P256dh { get; private set; } = string.Empty;
    public string Auth { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private PushSubscription() { }

    public PushSubscription(Guid userId, string endpoint, string p256dh, string auth, DateTime createdAt)
    {
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        CreatedAt = createdAt;
    }
}
