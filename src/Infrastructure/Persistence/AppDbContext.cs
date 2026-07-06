using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// ה-DbContext הראשי (Code-First). מחזיק את כל ה-aggregates.
/// היסטוריית הסטטוסים נשמרת כ-owned/related של Event ונכתבת יחד איתו בטרנזקציה.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventStatusHistory> EventStatusHistory => Set<EventStatusHistory>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EventSource> EventSources => Set<EventSource>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
