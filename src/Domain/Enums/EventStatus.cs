namespace Domain.Enums;

/// <summary>
/// מחזור החיים של אירוע שטח. ראה EventStateMachine לחוקי המעברים המותרים.
/// </summary>
public enum EventStatus
{
    /// <summary>אירוע נכנס למערכת, טרם הוקצה לטכנאי.</summary>
    New = 0,

    /// <summary>הוקצה לטכנאי על ידי הסדרן.</summary>
    Assigned = 1,

    /// <summary>הטכנאי בטיפול פעיל באירוע.</summary>
    InProgress = 2,

    /// <summary>מצב ביניים — ממתין למידע נוסף כדי להמשיך.</summary>
    WaitingForInfo = 3,

    /// <summary>האירוע טופל והושלם (מצב סופי).</summary>
    Completed = 4,

    /// <summary>האירוע בוטל (מצב סופי).</summary>
    Cancelled = 5
}
