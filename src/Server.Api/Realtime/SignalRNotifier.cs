using Application.Abstractions;
using Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Server.Api.Realtime;

/// <summary>
/// מימוש IRealtimeNotifier מעל SignalR. דוחף אירוע חדש לכל הסדרנים המחוברים
/// (event "NewEvent") — זה החלק שסוגר את ה-Flow הנדרש E2E (§10).
/// </summary>
public sealed class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<EventsHub> _hub;

    public SignalRNotifier(IHubContext<EventsHub> hub) => _hub = hub;

    public Task NotifyDispatchersNewEventAsync(EventDto @event, CancellationToken ct = default) =>
        _hub.Clients.Group(EventsHub.DispatchersGroup).SendAsync("NewEvent", @event, ct);
}
