using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Infrastructure.Persistence;
using TravelTogether.Infrastructure.Persistence.Repositories;

namespace TravelTogether.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<TravelTogetherDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TravelTogetherDbContext>());
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
