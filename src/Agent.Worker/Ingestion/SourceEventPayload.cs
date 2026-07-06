namespace Agent.Worker.Ingestion;

/// <summary>
/// המבנה שמקור חיצוני שולח ל-Agent (webhook). מכוון: כמות מידע קטנה בלבד (§1).
/// שים לב שאין כאן SourceId — זהות המקור נקבעת מהאימות (API Key), לא מהגוף.
/// </summary>
public sealed class SourceEventPayload
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int? Priority { get; set; }
}
