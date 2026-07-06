using Domain.Enums;

namespace Domain.Entities;

/// <summary>משתמש מערכת — סדרן או טכנאי.</summary>
public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>סטטוס חיבור בזמן אמת (נגזר מנוכחות SignalR). רלוונטי בעיקר לטכנאים.</summary>
    public bool IsConnected { get; set; }
    public DateTime? LastSeenAt { get; set; }

    private User() { }

    public User(string name, string username, string passwordHash, UserRole role, DateTime createdAt)
    {
        Name = name;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
    }
}
