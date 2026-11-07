namespace TravelTogether.Domain.Settlements;

/// <summary>How much a participant paid and how much of the trip's expenses is theirs.</summary>
/// <param name="ParticipantId">The participant.</param>
/// <param name="Paid">Total of the expenses this participant paid for.</param>
/// <param name="Share">Total of this participant's parts across all expense splits.</param>
public sealed record ParticipantBalance(Guid ParticipantId, decimal Paid, decimal Share)
{
    /// <summary>Positive: the group owes this participant. Negative: this participant owes the group.</summary>
    public decimal Net => Paid - Share;
}
