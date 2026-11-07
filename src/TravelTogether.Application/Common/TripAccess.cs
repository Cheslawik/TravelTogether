using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Application.Common.Exceptions;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Common;

internal static class TripAccess
{
    /// <summary>
    /// Loads a trip of the current user. A trip of another user is reported as not found,
    /// so the API does not reveal which ids exist.
    /// </summary>
    public static async Task<Trip> GetOwnTripAsync(
        this ITripRepository trips,
        Guid tripId,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        return await trips.GetAsync(tripId, currentUser.UserId, cancellationToken)
            ?? throw NotFoundException.For("Trip", tripId);
    }

    public static Participant FindParticipant(this Trip trip, Guid participantId)
    {
        return trip.Participants.FirstOrDefault(p => p.Id == participantId)
            ?? throw NotFoundException.For("Participant", participantId);
    }

    public static Expense FindExpense(this Trip trip, Guid expenseId)
    {
        return trip.Expenses.FirstOrDefault(e => e.Id == expenseId)
            ?? throw NotFoundException.For("Expense", expenseId);
    }
}
