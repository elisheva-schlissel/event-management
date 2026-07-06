using Domain.Enums;

namespace Domain.StateMachine;

/// <summary>
/// מגדיר את המעברים החוקיים בין מצבי אירוע ואוכף אותם.
/// המעברים מוגדרים בקוד (לא ניתן להגיע לכל מצב מכל מצב) — דרישת §5 באפיון.
/// חסר תלות בתשתית (EF/DB) → בר-בדיקה ביחידה ללא מסד נתונים.
/// </summary>
public static class EventStateMachine
{
    /// <summary>
    /// טבלת המעברים המותרים: לכל מצב, קבוצת המצבים שאליהם מותר לעבור ממנו.
    /// מצבים סופיים (Completed, Cancelled) ממופים לקבוצה ריקה.
    /// </summary>
    private static readonly IReadOnlyDictionary<EventStatus, IReadOnlySet<EventStatus>> Transitions =
        new Dictionary<EventStatus, IReadOnlySet<EventStatus>>
        {
            [EventStatus.New] = new HashSet<EventStatus>
            {
                EventStatus.Assigned, EventStatus.Cancelled
            },
            [EventStatus.Assigned] = new HashSet<EventStatus>
            {
                EventStatus.InProgress, EventStatus.Cancelled
            },
            [EventStatus.InProgress] = new HashSet<EventStatus>
            {
                EventStatus.WaitingForInfo, EventStatus.Completed, EventStatus.Cancelled
            },
            [EventStatus.WaitingForInfo] = new HashSet<EventStatus>
            {
                EventStatus.InProgress, EventStatus.Cancelled
            },
            [EventStatus.Completed] = new HashSet<EventStatus>(),
            [EventStatus.Cancelled] = new HashSet<EventStatus>()
        };

    /// <summary>האם מותר לעבור מ-<paramref name="from"/> ל-<paramref name="to"/>.</summary>
    public static bool CanTransition(EventStatus from, EventStatus to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    /// <summary>המצבים שאליהם ניתן לעבור ממצב נתון (שימושי ל-UI ולבדיקות).</summary>
    public static IReadOnlySet<EventStatus> AllowedTargets(EventStatus from) =>
        Transitions.TryGetValue(from, out var allowed)
            ? allowed
            : new HashSet<EventStatus>();

    /// <summary>מצב סופי שאין ממנו מעברים.</summary>
    public static bool IsTerminal(EventStatus status) => AllowedTargets(status).Count == 0;
}
