using TravelTogether.Domain.Common;

namespace TravelTogether.Domain.Entities;

/// <summary>A person travelling on a trip. Participants do not need a user account.</summary>
public sealed class Participant
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 254;

    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }

    private Participant()
    {
    }

    internal Participant(Guid tripId, string name, string? email)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        Update(name, email);
    }

    internal void Update(string name, string? email)
    {
        Name = Guard.RequiredText(name, NameMaxLength, "Participant name");
        Email = Guard.OptionalText(email, EmailMaxLength, "Participant email");
    }
}
