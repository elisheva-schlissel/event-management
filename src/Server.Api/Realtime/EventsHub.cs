using System.Security.Claims;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Server.Api.Realtime;

/// <summary>
/// Hub לתקשורת בזמן אמת. סדרנים מצטרפים לקבוצה "dispatchers" ומקבלים אירועים חדשים.
/// נוכחות המשתמש (connected/disconnected) מתעדכנת ב-DB — מקור האמת ל"מצב" (§5).
/// </summary>
[Authorize]
public sealed class EventsHub : Hub
{
    public const string DispatchersGroup = "dispatchers";

    private readonly AppDbContext _db;

    public EventsHub(AppDbContext db, ILogger<EventsHub> logger)
    {
        _db = db;
        _logger = logger;
    }

    private readonly ILogger<EventsHub> _logger;

    public override async Task OnConnectedAsync()
    {
        var isDispatcher = Context.User?.IsInRole(nameof(UserRole.Dispatcher)) ?? false;
        _logger.LogInformation("Hub connect: conn={Conn} isDispatcher={IsDispatcher}",
            Context.ConnectionId, isDispatcher);

        if (isDispatcher)
            await Groups.AddToGroupAsync(Context.ConnectionId, DispatchersGroup);

        await SetPresenceAsync(true);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await SetPresenceAsync(false);
        await base.OnDisconnectedAsync(exception);
    }

    private async Task SetPresenceAsync(bool connected)
    {
        var idClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var userId)) return;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        user.IsConnected = connected;
        user.LastSeenAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
