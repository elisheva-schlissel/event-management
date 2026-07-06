using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Api.Controllers;

/// <summary>
/// SKELETON (§11): פעולות שאינן חלק מה-Flow היחיד שמומש E2E.
/// המבנה, ההרשאות וה-endpoints מוגדרים; הלוגיקה העסקית תושלם בהמשך.
/// כל endpoint מחזיר 501 Not Implemented עם תיאור הכוונה.
/// </summary>
[ApiController]
[Route("api/events")]
[Authorize]
public sealed class EventActionsController : ControllerBase
{
    /// <summary>הסדרן מקצה אירוע לטכנאי (New → Assigned) + התראה לטכנאי.</summary>
    [Authorize(Roles = nameof(UserRole.Dispatcher))]
    [HttpPost("{id:guid}/assign")]
    public IActionResult Assign(Guid id, [FromBody] AssignRequest body) =>
        StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Skeleton: יקרא ל-Event.AssignTo + INotifier לטכנאי" });

    /// <summary>העברת אירוע מטכנאי לטכנאי — שני הצדדים מקבלים עדכון.</summary>
    [Authorize(Roles = nameof(UserRole.Dispatcher))]
    [HttpPost("{id:guid}/transfer")]
    public IActionResult Transfer(Guid id, [FromBody] TransferRequest body) =>
        StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Skeleton: יקרא ל-Event.Reassign + עדכון שני הטכנאים" });

    /// <summary>הטכנאי מעדכן סטטוס אירוע (מעבר מוגן ב-State Machine).</summary>
    [Authorize(Roles = nameof(UserRole.Technician))]
    [HttpPost("{id:guid}/status")]
    public IActionResult ChangeStatus(Guid id, [FromBody] ChangeStatusRequest body) =>
        StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Skeleton: יקרא ל-Event.ChangeStatus עם אכיפת בעלות" });

    /// <summary>הטכנאי מבקש לקבל על עצמו אירוע פנוי.</summary>
    [Authorize(Roles = nameof(UserRole.Technician))]
    [HttpPost("{id:guid}/claim")]
    public IActionResult Claim(Guid id) =>
        StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Skeleton: הקצאה עצמית של אירוע פנוי" });

    /// <summary>הטכנאי שולח הערה/עדכון לסדרן על אירוע פעיל.</summary>
    [Authorize(Roles = nameof(UserRole.Technician))]
    [HttpPost("{id:guid}/note")]
    public IActionResult AddNote(Guid id, [FromBody] NoteRequest body) =>
        StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Skeleton: הוספת הערה + התראה לסדרן" });
}

public sealed record AssignRequest(Guid TechnicianId);
public sealed record TransferRequest(Guid ToTechnicianId);
public sealed record ChangeStatusRequest(string NewStatus, string? Note);
public sealed record NoteRequest(string Text);
