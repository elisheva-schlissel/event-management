namespace Domain.Entities;

/// <summary>
/// מקור חיצוני שרשאי לדווח על אירועים ל-Agent (חיישן, מערכת, טופס).
/// האימות מבוסס API Key + HMAC על ה-secret (ראה §8 בתוכנית).
/// </summary>
public class EventSource
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;

    /// <summary>מזהה פומבי שהמקור שולח בכותרת X-Api-Key.</summary>
    public string ApiKey { get; private set; } = string.Empty;

    /// <summary>הסוד לחתימת HMAC — נשמר מוצפן/hashed, לעולם לא נחשף חזרה.</summary>
    public string SecretHash { get; private set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }

    private EventSource() { }

    public EventSource(string name, string apiKey, string secretHash, DateTime createdAt, Guid? id = null)
    {
        if (id is not null) Id = id.Value;
        Name = name;
        ApiKey = apiKey;
        SecretHash = secretHash;
        CreatedAt = createdAt;
    }
}
