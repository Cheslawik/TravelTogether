using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Common.Abstractions;

public interface ITripRepository
{
    /// <summary>Loads the whole trip aggregate (participants, expenses, splits) if it belongs to the owner.</summary>
    Task<Trip?> GetAsync(Guid tripId, Guid ownerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TripSummary>> ListAsync(Guid ownerId, CancellationToken cancellationToken);

    void Add(Trip trip);

    void Remove(Trip trip);
}

/// <summary>Lightweight read model for the list of trips.</summary>
public sealed record TripSummary(
    Guid Id,
    string Title,
    string Currency,
    DateOnly StartDate,
    DateOnly EndDate,
    int ParticipantCount,
    int ExpenseCount);
