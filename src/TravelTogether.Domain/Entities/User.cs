using TravelTogether.Domain.Common;

namespace TravelTogether.Domain.Entities;

/// <summary>An account that owns trips. Passwords are stored only as a hash.</summary>
public sealed class User
{
    public const int EmailMaxLength = 254;
    public const int DisplayNameMaxLength = 100;

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public User(string email, string displayName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        DisplayName = Guard.RequiredText(displayName, DisplayNameMaxLength, "Display name");
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Emails are compared case-insensitively, so they are stored in lower case.</summary>
    public static string NormalizeEmail(string email)
    {
        return Guard.RequiredText(email, EmailMaxLength, "Email").ToLowerInvariant();
    }
}
