using Microsoft.EntityFrameworkCore;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Infrastructure.Persistence
{
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
}
