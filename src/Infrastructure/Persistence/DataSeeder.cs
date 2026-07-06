using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// זרעים ראשוניים להרצה מיידית: סדרן, טכנאי ומקור אירועים אחד.
/// מאפשר להדגים את ה-Flow ללא הזנת נתונים ידנית (ראה README).
/// </summary>
public static class DataSeeder
{
    // ערכי דמו קבועים — מתועדים ב-README. בפרודקשן ייווצרו דינמית.
    public static readonly Guid SourceId = new("11111111-1111-1111-1111-111111111111");
    public const string SourceApiKey = "demo-source-key";
    public const string SourceSecret = "demo-source-secret";
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, IClock clock, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!await db.Users.AnyAsync(ct))
        {
            db.Users.Add(new User("שרה הסדרנית", "dispatcher", hasher.Hash(DemoPassword), UserRole.Dispatcher, clock.UtcNow));
            db.Users.Add(new User("טל הטכנאי", "tech1", hasher.Hash(DemoPassword), UserRole.Technician, clock.UtcNow));
        }

        if (!await db.EventSources.AnyAsync(ct))
        {
            db.EventSources.Add(new EventSource(
                "רשת חיישנים", SourceApiKey, hasher.Hash(SourceSecret), clock.UtcNow, SourceId));
        }

        await db.SaveChangesAsync(ct);
    }
}
