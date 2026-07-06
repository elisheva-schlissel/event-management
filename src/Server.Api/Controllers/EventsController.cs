using Application.Events;
using Contracts;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db) => _db = db;

    /// <summary>
    /// קליטת אירוע מה-Agent (חלק מה-Flow הנדרש E2E).
    /// אימות פנימי בין Agent לשרת דרך כותרת X-Agent-Key (ראה AgentAuthFilter).
    /// </summary>
    [ServiceFilter(typeof(AgentAuthFilter))]
    [HttpPost("ingest")]
    public async Task<ActionResult<IngestEventResponse>> Ingest(
        [FromServices] EventIngestionService ingestion,
        IngestEventRequest request,
        CancellationToken ct)
    {
        var response = await ingestion.IngestAsync(request, ct);
        return response.Created
            ? CreatedAtAction(nameof(GetById), new { id = response.EventId }, response)
            : Ok(response); // idempotent replay — 200 ללא יצירה כפולה
    }

    /// <summary>הסדרן רואה את כל האירועים הפעילים (§3).</summary>
    [Authorize(Roles = nameof(UserRole.Dispatcher))]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAll(CancellationToken ct)
    {
        var events = await _db.Events
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToListAsync(ct);
        return Ok(events);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken ct)
    {
        var e = await _db.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? NotFound() : Ok(e.ToDto());
    }

    /// <summary>היסטוריית המצבים של אירוע — מוצגת ב-UI (§6).</summary>
    [Authorize]
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<object>>> GetHistory(Guid id, CancellationToken ct)
    {
        var history = await _db.EventStatusHistory
            .Where(h => h.EventId == id)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new
            {
                h.FromStatus,
                h.ToStatus,
                h.ChangedByUserId,
                h.ChangedAt,
                h.Note
            })
            .ToListAsync(ct);
        return Ok(history);
    }
}
