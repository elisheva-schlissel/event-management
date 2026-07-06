using System.ComponentModel.DataAnnotations;

namespace Contracts;

/// <summary>
/// חוזה קליטת אירוע מה-Agent לשרת (POST /api/events/ingest).
/// ה-DataAnnotations נותנים validation אוטומטי בצד השרת (400 על קלט חסר) —
/// זה מה שמחליף את ה"חוזה החזק" של gRPC בפתרון מבוסס REST (ראה §18.0 בתוכנית).
/// </summary>
public sealed class IngestEventRequest
{
    /// <summary>מפתח ייחודי מה-Agent למניעת כפילות בשידור חוזר.</summary>
    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Location { get; set; }

    /// <summary>מזהה המקור המדווח (EventSource) כפי שאומת ב-Agent.</summary>
    [Required]
    public Guid SourceId { get; set; }

    /// <summary>עדיפות אופציונלית (0=Low,1=Normal,2=High,3=Critical).</summary>
    public int Priority { get; set; } = 1;
}
