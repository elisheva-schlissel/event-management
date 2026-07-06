namespace Contracts;

/// <summary>תגובת השרת לקליטת אירוע.</summary>
public sealed class IngestEventResponse
{
    public Guid EventId { get; set; }

    /// <summary>true אם האירוע נקלט עכשיו; false אם כבר היה קיים (idempotent replay).</summary>
    public bool Created { get; set; }
}
