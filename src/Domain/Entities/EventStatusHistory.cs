using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// רשומת היסטוריה של שינוי סטטוס. כל מעבר נשמר עם חותמת זמן ומשתמש מבצע
/// (דרישת §5 באפיון: "כל שינוי סטטוס נשמר עם חותמת זמן ומשתמש מבצע").
/// </summary>
public class EventStatusHistory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EventId { get; private set; }

    /// <summary>המצב הקודם. null עבור הרשומה הראשונה (יצירת האירוע).</summary>
    public EventStatus? FromStatus { get; private set; }
    public EventStatus ToStatus { get; private set; }

    /// <summary>המשתמש שביצע את השינוי. null כשהמערכת/Agent יצרו את האירוע.</summary>
    public Guid? ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? Note { get; private set; }

    private EventStatusHistory() { }

    public EventStatusHistory(
        Guid eventId,
        EventStatus? fromStatus,
        EventStatus toStatus,
        Guid? changedByUserId,
        DateTime changedAt,
        string? note)
    {
        EventId = eventId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedByUserId = changedByUserId;
        ChangedAt = changedAt;
        Note = note;
    }
}
