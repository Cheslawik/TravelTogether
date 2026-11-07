using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TravelTogether.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    /// <summary>Applies pending EF Core migrations. Used on startup in Docker and local development.</summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TravelTogetherDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
