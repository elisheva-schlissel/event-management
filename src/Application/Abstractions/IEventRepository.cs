using Domain.Entities;

namespace Application.Abstractions;

/// <summary>
/// Port לגישה לאירועים. המימוש (EF Core) יושב ב-Infrastructure —
/// כך ה-Application וה-Domain נשארים ללא תלות במסד הנתונים.
/// </summary>
public interface IEventRepository
{
    /// <summary>מאתר אירוע לפי מפתח ה-idempotency (למניעת כפילויות מ-Agent).</summary>
    Task<Event?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);

    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Event @event, CancellationToken ct = default);

    /// <summary>שומר את כל השינויים בטרנזקציה אחת (אירוע + היסטוריה יחד).</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
