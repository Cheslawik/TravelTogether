using FluentValidation;
using TravelTogether.Application.Common;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Application.Participants;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Trips;

public sealed class TripService(
    ITripRepository trips,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IValidator<TripRequest> validator)
{
    public async Task<IReadOnlyList<TripSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        var summaries = await trips.ListAsync(currentUser.UserId, cancellationToken);

        return summaries
            .Select(t => new TripSummaryDto(
                t.Id, t.Title, t.Currency, t.StartDate, t.EndDate, t.ParticipantCount, t.ExpenseCount))
            .ToArray();
    }

    public async Task<TripDto> GetAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        return ToDto(trip);
    }

    public async Task<TripDto> CreateAsync(TripRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var trip = new Trip(
            currentUser.UserId,
            request.Title,
            request.Description,
            request.Currency,
            request.StartDate,
            request.EndDate);

        trips.Add(trip);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(trip);
    }

    public async Task<TripDto> UpdateAsync(Guid tripId, TripRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);

        trip.UpdateDetails(request.Title, request.Description, request.StartDate, request.EndDate);
        trip.ChangeCurrency(request.Currency);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(trip);
    }

    public async Task DeleteAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);

        trips.Remove(trip);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static TripDto ToDto(Trip trip) => new(
        trip.Id,
        trip.Title,
        trip.Description,
        trip.Currency,
        trip.StartDate,
        trip.EndDate,
        trip.Expenses.Sum(e => e.Amount),
        trip.Participants.Select(ParticipantDto.From).ToArray());
}
