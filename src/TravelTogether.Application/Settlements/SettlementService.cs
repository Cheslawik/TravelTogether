using TravelTogether.Application.Common;
using TravelTogether.Application.Common.Abstractions;

namespace TravelTogether.Application.Settlements;

public sealed class SettlementService(ITripRepository trips, ICurrentUser currentUser)
{
    public async Task<BalancesDto> GetBalancesAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        var names = trip.Participants.ToDictionary(p => p.Id, p => p.Name);

        var balances = trip.CalculateBalances()
            .Select(b => new BalanceDto(b.ParticipantId, names[b.ParticipantId], b.Paid, b.Share, b.Net))
            .OrderByDescending(b => b.Net)
            .ToArray();

        return new BalancesDto(trip.Currency, balances);
    }

    public async Task<SettlementDto> GetSettlementAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        var names = trip.Participants.ToDictionary(p => p.Id, p => p.Name);

        var transfers = trip.CalculateSettlement()
            .Select(t => new TransferDto(
                t.FromParticipantId,
                names[t.FromParticipantId],
                t.ToParticipantId,
                names[t.ToParticipantId],
                t.Amount))
            .ToArray();

        return new SettlementDto(trip.Currency, transfers);
    }
}
