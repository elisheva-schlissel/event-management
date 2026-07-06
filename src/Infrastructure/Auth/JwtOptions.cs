namespace Infrastructure.Auth;

/// <summary>הגדרות JWT הנטענות מ-configuration (section "Jwt").</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "FieldEventManagement";
    public string Audience { get; set; } = "FieldEventManagement.Client";

    /// <summary>מפתח סימטרי לחתימה — נטען מ-user-secrets/env, לא מהקוד.</summary>
    public string Secret { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
