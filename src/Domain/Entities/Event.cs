using Domain.Enums;
using Domain.Exceptions;
using Domain.StateMachine;

namespace Domain.Entities;

/// <summary>
/// אירוע שטח — ה-aggregate root של המערכת.
/// כל שינוי סטטוס עובר דרך ChangeStatus שאוכף את חוקי ה-EventStateMachine
/// ומוסיף רשומת היסטוריה. אין setter ציבורי ל-Status — לא ניתן לעקוף את החוקים.
/// </summary>
public class Event
{
    private readonly List<EventStatusHistory> _statusHistory = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Location { get; private set; }

    /// <summary>מזהה המקור המדווח (EventSource).</summary>
    public Guid SourceId { get; private set; }

    /// <summary>
    /// מפתח ייחודי מה-Agent למניעת כפילויות בשידור חוזר (idempotency, ראה §4/§10).
    /// </summary>
    public string IdempotencyKey { get; private set; } = string.Empty;

    public EventStatus Status { get; private set; }
    public EventPriority Priority { get; private set; } = EventPriority.Normal;
    public Guid? AssignedTechnicianId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<EventStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Event() { }

    /// <summary>
    /// יוצר אירוע חדש במצב New ורושם את רשומת ההיסטוריה הראשונה.
    /// זו נקודת הכניסה בזמן קליטת אירוע מה-Agent.
    /// </summary>
    public static Event Create(
        string title,
        string description,
        string? location,
        Guid sourceId,
        string idempotencyKey,
        DateTime createdAt,
        EventPriority priority = EventPriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("כותרת אירוע היא שדה חובה.", nameof(title));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("IdempotencyKey הוא שדה חובה.", nameof(idempotencyKey));

        var @event = new Event
        {
            Title = title,
            Description = description,
            Location = location,
            SourceId = sourceId,
            IdempotencyKey = idempotencyKey,
            Priority = priority,
            Status = EventStatus.New,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        @event._statusHistory.Add(new EventStatusHistory(
            @event.Id, fromStatus: null, toStatus: EventStatus.New,
            changedByUserId: null, changedAt: createdAt, note: "האירוע נוצר"));

        return @event;
    }

    /// <summary>
    /// מבצע מעבר סטטוס מוגן. זורק InvalidEventTransitionException אם המעבר אסור.
    /// כל מעבר מוסיף רשומת היסטוריה עם המשתמש המבצע וחותמת הזמן.
    /// </summary>
    public void ChangeStatus(EventStatus newStatus, Guid? byUserId, DateTime at, string? note = null)
    {
        if (!EventStateMachine.CanTransition(Status, newStatus))
            throw new InvalidEventTransitionException(Status, newStatus);

        var previous = Status;
        Status = newStatus;
        UpdatedAt = at;

        _statusHistory.Add(new EventStatusHistory(
            Id, fromStatus: previous, toStatus: newStatus,
            changedByUserId: byUserId, changedAt: at, note: note));
    }

    /// <summary>הקצאת האירוע לטכנאי (מעבר New → Assigned).</summary>
    public void AssignTo(Guid technicianId, Guid byDispatcherId, DateTime at, string? note = null)
    {
        ChangeStatus(EventStatus.Assigned, byDispatcherId, at, note);
        AssignedTechnicianId = technicianId;
    }

    /// <summary>העברת האירוע מטכנאי לטכנאי (נשאר במצב Assigned/InProgress).</summary>
    public void Reassign(Guid newTechnicianId, Guid byDispatcherId, DateTime at)
    {
        AssignedTechnicianId = newTechnicianId;
        UpdatedAt = at;
    }

    /// <summary>הסדרן מסמן עדיפות.</summary>
    public void SetPriority(EventPriority priority, DateTime at)
    {
        Priority = priority;
        UpdatedAt = at;
    }
}
