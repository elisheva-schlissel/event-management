using Microsoft.EntityFrameworkCore;

namespace Agent.Worker.Outbox;

/// <summary>DbContext מקומי של ה-Agent מעל SQLite — מחזיק את ה-Outbox בלבד.</summary>
public sealed class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> Messages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<OutboxMessage>();
        b.HasKey(m => m.Id);
        b.Property(m => m.IdempotencyKey).IsRequired();
        b.HasIndex(m => m.IdempotencyKey).IsUnique();
        b.Property(m => m.PayloadJson).IsRequired();
        b.Property(m => m.Status).HasConversion<string>();
        b.HasIndex(m => m.Status);
    }
}
