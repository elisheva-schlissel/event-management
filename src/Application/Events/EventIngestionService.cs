using Application.Abstractions;
using Contracts;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Events;

/// <summary>
/// ה-use-case המרכזי של ה-Flow הנדרש E2E (§10):
/// קליטת אירוע מה-Agent → בדיקת idempotency → שמירה ל-DB → התראה בזמן אמת לסדרנים.
/// </summary>
public sealed class EventIngestionService
{
    private readonly IEventRepository _repository;
    private readonly IRealtimeNotifier _notifier;
    private readonly IClock _clock;
    private readonly ILogger<EventIngestionService> _logger;

    public EventIngestionService(
        IEventRepository repository,
        IRealtimeNotifier notifier,
        IClock clock,
        ILogger<EventIngestionService> logger)
    {
        _repository = repository;
        _notifier = notifier;
        _clock = clock;
        _logger = logger;
    }

    public async Task<IngestEventResponse> IngestAsync(IngestEventRequest request, CancellationToken ct = default)
    {
        // Idempotency: שידור חוזר מה-Agent (at-least-once) לא ייצור כפילות.
        var existing = await _repository.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
        {
            _logger.LogInformation(
                "אירוע עם IdempotencyKey {Key} כבר קיים (Id={Id}) — מדלג.",
                request.IdempotencyKey, existing.Id);
            return new IngestEventResponse { EventId = existing.Id, Created = false };
        }

        var priority = Enum.IsDefined(typeof(EventPriority), request.Priority)
            ? (EventPriority)request.Priority
            : EventPriority.Normal;

        var @event = Event.Create(
            request.Title,
            request.Description,
            request.Location,
            request.SourceId,
            request.IdempotencyKey,
            _clock.UtcNow,
            priority);

        await _repository.AddAsync(@event, ct);
        await _repository.SaveChangesAsync(ct); // אירוע + היסטוריה ראשונית בטרנזקציה אחת

        _logger.LogInformation("אירוע חדש נקלט Id={Id} Title={Title}", @event.Id, @event.Title);

        // התראה בזמן אמת לסדרנים המחוברים — סוגר את ה-Flow.
        await _notifier.NotifyDispatchersNewEventAsync(@event.ToDto(), ct);

        return new IngestEventResponse { EventId = @event.Id, Created = true };
    }
}
