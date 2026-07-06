using Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// STUB (§11): מימוש Web Push למשתמש שאינו מחובר. במסגרת המבחן לא נשלחת התראה בפועל —
/// רק מתועד הזרימה. מימוש מלא יחתום עם VAPID וישלח POST ל-endpoint של הדפדפן
/// עבור כל PushSubscription של המשתמש (ראה §5, §18.ד בתוכנית).
/// </summary>
public sealed class WebPushNotifierStub : IPushNotifier
{
    private readonly ILogger<WebPushNotifierStub> _logger;

    public WebPushNotifierStub(ILogger<WebPushNotifierStub> logger) => _logger = logger;

    public Task SendAsync(Guid userId, string title, string body, string? deepLink, CancellationToken ct = default)
    {
        // TODO (production): טעינת PushSubscriptions של userId מה-DB,
        // חתימת VAPID, ושליחת encrypted payload ל-endpoint של כל דפדפן.
        _logger.LogInformation(
            "[WebPush STUB] היה נשלח Push ל-User={UserId}: '{Title}' → {DeepLink}",
            userId, title, deepLink);
        return Task.CompletedTask;
    }
}
