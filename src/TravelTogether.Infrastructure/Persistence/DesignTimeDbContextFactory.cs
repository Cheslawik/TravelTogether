using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelTogether.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef</c> create the context without starting the web app.
/// The connection string is only used when commands like <c>database update</c> are run.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TravelTogetherDbContext>
{
    public TravelTogetherDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=traveltogether;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<TravelTogetherDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TravelTogetherDbContext(options);
    }
}
