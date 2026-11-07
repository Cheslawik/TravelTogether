using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Participants;

public sealed record ParticipantRequest(string Name, string? Email);

public sealed record ParticipantDto(Guid Id, string Name, string? Email)
{
    public static ParticipantDto From(Participant participant) =>
        new(participant.Id, participant.Name, participant.Email);
}
