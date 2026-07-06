using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>מימוש EF Core של IEventRepository.</summary>
public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default) =>
        _db.Events.FirstOrDefaultAsync(e => e.IdempotencyKey == idempotencyKey, ct);

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Events
            .Include(e => e.StatusHistory)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Event @event, CancellationToken ct = default) =>
        await _db.Events.AddAsync(@event, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
