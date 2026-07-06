using System.Security.Cryptography;
using System.Text;

namespace Agent.Worker.Security;

/// <summary>
/// מאמת מקורות חיצוניים המדווחים ל-Agent (§8): API Key + חתימת HMAC על הגוף + timestamp
/// נגד replay. שונה מאימות המשתמשים (JWT) כי מקור הוא מכונה עם סוד קבוע, לא login אנושי.
/// </summary>
public sealed class SourceAuthenticator
{
    private readonly SourceAuthOptions _options;

    public SourceAuthenticator(SourceAuthOptions options) => _options = options;

    public bool IsValid(string apiKey, string timestamp, string signature, string rawBody, out string? error)
    {
        error = null;

        if (apiKey != _options.ApiKey)
        {
            error = "API Key לא מוכר";
            return false;
        }

        if (!long.TryParse(timestamp, out var unixSeconds))
        {
            error = "Timestamp לא תקין";
            return false;
        }

        // anti-replay: דוחים בקשות ישנות/עתידיות מדי (חלון של 5 דקות).
        var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if (Math.Abs(age.TotalMinutes) > 5)
        {
            error = "Timestamp מחוץ לחלון המותר (replay?)";
            return false;
        }

        var expected = ComputeSignature(_options.Secret, timestamp, rawBody);
        var provided = Convert.FromHexString(SafeHex(signature));
        if (provided.Length == 0 ||
            !CryptographicOperations.FixedTimeEquals(provided, Convert.FromHexString(expected)))
        {
            error = "חתימת HMAC שגויה";
            return false;
        }

        return true;
    }

    public static string ComputeSignature(string secret, string timestamp, string rawBody)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{rawBody}"));
        return Convert.ToHexString(hash);
    }

    private static string SafeHex(string s)
    {
        try { _ = Convert.FromHexString(s); return s; }
        catch { return string.Empty; }
    }
}

/// <summary>הגדרות המקור המורשה (נטען מ-configuration). בפרודקשן — טבלת EventSources.</summary>
public sealed class SourceAuthOptions
{
    public const string SectionName = "SourceAuth";
    public string ApiKey { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
}
