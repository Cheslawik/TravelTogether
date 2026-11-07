using System.Threading.RateLimiting;

namespace TravelTogether.WebApi.Extensions;

internal static class RateLimitPolicies
{
    public const string Auth = "auth";

    /// <summary>Limits login/registration attempts per client IP to slow down password guessing.</summary>
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:AuthPermitsPerMinute", 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(Auth, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromMinutes(1),
                }));
        });

        return services;
    }
}
