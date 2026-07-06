using Contracts;

namespace Application.Abstractions;

/// <summary>
/// Port להתראות בזמן אמת. המימוש הפעיל (SignalR) יושב ב-Infrastructure.
/// ההפרדה מאפשרת גם מימוש stub ל-Web Push ללא שינוי ב-use-case (§5, §11).
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>דוחף אירוע חדש לכל הסדרנים המחוברים (חלק מה-Flow הנדרש E2E).</summary>
    Task NotifyDispatchersNewEventAsync(EventDto @event, CancellationToken ct = default);
}
