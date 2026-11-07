namespace TravelTogether.Domain.Splitting;

/// <summary>
/// A participant taking part in an expense and the weight of their part.
/// For an equal split the weight is ignored.
/// </summary>
public sealed record ExpenseShare(Guid ParticipantId, int Shares = 1);
