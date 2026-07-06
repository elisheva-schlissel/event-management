namespace Domain.Enums;

/// <summary>עדיפות האירוע. הסדרן יכול לסמן עדיפות (ראה §3 באפיון).</summary>
public enum EventPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
