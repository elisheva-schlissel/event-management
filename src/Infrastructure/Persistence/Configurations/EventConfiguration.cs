using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("Events");
        b.HasKey(e => e.Id);

        b.Property(e => e.Title).IsRequired().HasMaxLength(200);
        b.Property(e => e.Description).HasMaxLength(2000);
        b.Property(e => e.Location).HasMaxLength(300);
        b.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(200);
        b.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);

        // מונע כפילות אירועים משידור חוזר של ה-Agent (idempotency).
        b.HasIndex(e => e.IdempotencyKey).IsUnique();
        b.HasIndex(e => e.Status);
        b.HasIndex(e => e.AssignedTechnicianId);

        // מיפוי ההיסטוריה דרך ה-backing field הפרטי _statusHistory.
        var nav = b.Metadata.FindNavigation(nameof(Event.StatusHistory))!;
        nav.SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(e => e.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
