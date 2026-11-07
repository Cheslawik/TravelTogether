using Microsoft.EntityFrameworkCore;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence;

public class TravelTogetherDbContext(
    DbContextOptions<TravelTogetherDbContext> options)
    : DbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TravelTogetherDbContext).Assembly);
    }
}
