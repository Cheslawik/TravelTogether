using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Infrastructure.Authentication;
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

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.SigningKey.Length >= 32, "Jwt:SigningKey must be at least 32 characters long.")
            .Validate(o => o.AccessTokenLifetimeMinutes > 0, "Jwt:AccessTokenLifetimeMinutes must be positive.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
