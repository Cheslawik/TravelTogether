using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using TravelTogether.Application;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Infrastructure;
using TravelTogether.Infrastructure.Persistence;
using TravelTogether.WebApi.ErrorHandling;
using TravelTogether.WebApi.Extensions;
using TravelTogether.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthRateLimiting(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddHealthChecks().AddDbContextCheck<TravelTogetherDbContext>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await app.Services.ApplyMigrationsAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "TravelTogether API");
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health").WithMetadata(new AllowAnonymousAttribute());

await app.RunAsync();
