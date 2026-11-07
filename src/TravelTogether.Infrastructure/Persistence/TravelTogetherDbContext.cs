using Microsoft.EntityFrameworkCore;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence;

public sealed class TravelTogetherDbContext(DbContextOptions<TravelTogetherDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Trip> Trips => Set<Trip>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelTogetherDbContext).Assembly);
    }
}
