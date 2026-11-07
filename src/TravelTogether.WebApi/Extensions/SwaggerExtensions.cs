using Microsoft.OpenApi;

namespace TravelTogether.WebApi.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TravelTogether API",
                Version = "v1",
                Description = "Split shared trip expenses and find out who owes whom.",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste the accessToken returned by /api/auth/login.",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });

            foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "TravelTogether.*.xml"))
            {
                options.IncludeXmlComments(xml);
            }
        });

        return services;
    }
}
