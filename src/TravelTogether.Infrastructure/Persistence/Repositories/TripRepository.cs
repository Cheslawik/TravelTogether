using Microsoft.EntityFrameworkCore;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence.Repositories;

internal sealed class TripRepository(TravelTogetherDbContext db) : ITripRepository
{
    public Task<Trip?> GetAsync(Guid tripId, Guid ownerId, CancellationToken cancellationToken)
    {
        return db.Trips
            .Include(t => t.Participants.OrderBy(p => p.Name))
            .Include(t => t.Expenses)
                .ThenInclude(e => e.Splits)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == tripId && t.OwnerId == ownerId, cancellationToken);
    }

    public async Task<IReadOnlyList<TripSummary>> ListAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await db.Trips
            .AsNoTracking()
            .Where(t => t.OwnerId == ownerId)
            .OrderByDescending(t => t.StartDate)
            .Select(t => new TripSummary(
                t.Id,
                t.Title,
                t.Currency,
                t.StartDate,
                t.EndDate,
                t.Participants.Count,
                t.Expenses.Count))
            .ToListAsync(cancellationToken);
    }

    public void Add(Trip trip) => db.Trips.Add(trip);

    public void Remove(Trip trip) => db.Trips.Remove(trip);
}
