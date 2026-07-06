using Application.Abstractions;

namespace Infrastructure.Services;

/// <summary>מימוש IClock מבוסס שעון המערכת.</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
