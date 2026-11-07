using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TravelTogether.Application.Auth;
using TravelTogether.Application.Expenses;
using TravelTogether.Application.Participants;
using TravelTogether.Application.Settlements;
using TravelTogether.Application.Trips;

namespace TravelTogether.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<AuthService>();
        services.AddScoped<TripService>();
        services.AddScoped<ParticipantService>();
        services.AddScoped<ExpenseService>();
        services.AddScoped<SettlementService>();

        return services;
    }
}
