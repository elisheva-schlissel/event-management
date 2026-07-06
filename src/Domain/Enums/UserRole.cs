namespace Domain.Enums;

/// <summary>
/// סוגי המשתמשים במערכת. ההרשאות נאכפות בצד השרת (ראה §8 בתוכנית).
/// </summary>
public enum UserRole
{
    /// <summary>סדרן / מנהל — רואה את כל האירועים, מקצה ומעביר.</summary>
    Dispatcher = 0,

    /// <summary>טכנאי / מפעיל שטח — רואה אך ורק את האירועים שהוקצו לו.</summary>
    Technician = 1
}
