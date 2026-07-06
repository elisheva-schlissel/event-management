namespace Application.Abstractions;

/// <summary>הפשטת זמן — מאפשרת דטרמיניזם בבדיקות במקום DateTime.UtcNow ישיר.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
