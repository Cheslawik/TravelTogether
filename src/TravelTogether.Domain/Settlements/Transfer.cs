namespace TravelTogether.Domain.Settlements;

/// <summary>A payment that settles debts: <paramref name="FromParticipantId"/> pays <paramref name="Amount"/> to <paramref name="ToParticipantId"/>.</summary>
public sealed record Transfer(Guid FromParticipantId, Guid ToParticipantId, decimal Amount);
