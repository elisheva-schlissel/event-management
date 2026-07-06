using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class EventStatusHistoryConfiguration : IEntityTypeConfiguration<EventStatusHistory>
{
    public void Configure(EntityTypeBuilder<EventStatusHistory> b)
    {
        b.ToTable("EventStatusHistory");
        b.HasKey(h => h.Id);
        b.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(30);
        b.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(30);
        b.Property(h => h.Note).HasMaxLength(1000);
        b.HasIndex(h => h.EventId);
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Name).IsRequired().HasMaxLength(150);
        b.Property(u => u.Username).IsRequired().HasMaxLength(100);
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(u => u.Username).IsUnique();
    }
}

public sealed class EventSourceConfiguration : IEntityTypeConfiguration<EventSource>
{
    public void Configure(EntityTypeBuilder<EventSource> b)
    {
        b.ToTable("EventSources");
        b.HasKey(s => s.Id);
        b.Property(s => s.Name).IsRequired().HasMaxLength(150);
        b.Property(s => s.ApiKey).IsRequired().HasMaxLength(100);
        b.Property(s => s.SecretHash).IsRequired();
        b.HasIndex(s => s.ApiKey).IsUnique();
    }
}

public sealed class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> b)
    {
        b.ToTable("PushSubscriptions");
        b.HasKey(p => p.Id);
        b.Property(p => p.Endpoint).IsRequired().HasMaxLength(500);
        b.Property(p => p.P256dh).IsRequired();
        b.Property(p => p.Auth).IsRequired();
        b.HasIndex(p => p.UserId);
    }
}
