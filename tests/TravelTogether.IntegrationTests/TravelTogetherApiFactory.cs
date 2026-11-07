using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TravelTogether.Infrastructure.Persistence;

namespace TravelTogether.IntegrationTests;

/// <summary>
/// Runs the real API in memory. PostgreSQL is replaced with an in-memory SQLite
/// database so the tests need no Docker and no external services.
/// </summary>
public sealed class TravelTogetherApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Database", "unused-in-tests");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-0123456789abcdef");
        builder.UseSetting("RateLimiting:AuthPermitsPerMinute", "1000");
        builder.UseSetting("Swagger:Enabled", "true");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<TravelTogetherDbContext>>();
            services.RemoveAll<DbContextOptions<TravelTogetherDbContext>>();

            _connection.Open();
            services.AddDbContext<TravelTogetherDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TravelTogetherDbContext>().Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
