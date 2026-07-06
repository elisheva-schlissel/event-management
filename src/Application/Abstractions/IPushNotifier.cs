namespace Application.Abstractions;

/// <summary>
/// Port להתראות Web Push למשתמש שאינו מחובר (דפדפן סגור).
/// במסגרת המבחן נשאר כ-stub מתועד (§5, §11) — החתימה מוגדרת, המימוש המלא עתידי.
/// </summary>
public interface IPushNotifier
{
    /// <summary>שולח התראת Web Push לכל המנויים של המשתמש.</summary>
    Task SendAsync(Guid userId, string title, string body, string? deepLink, CancellationToken ct = default);
}
