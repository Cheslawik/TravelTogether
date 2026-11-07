using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TravelTogether.Application.Auth;
using TravelTogether.Application.Expenses;
using TravelTogether.Application.Participants;
using TravelTogether.Application.Trips;

namespace TravelTogether.IntegrationTests;

internal static class ApiClientExtensions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<AuthResponse> RegisterAsync(this HttpClient client, string? email = null)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email ?? $"{Guid.NewGuid():N}@example.com", "Passw0rd!", "Test user"),
            Json);

        return await response.ReadAsync<AuthResponse>(HttpStatusCode.Created);
    }

    /// <summary>Registers a new user and returns a client that sends their token.</summary>
    public static async Task<HttpClient> AuthenticatedClientAsync(this TravelTogetherApiFactory factory)
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }

    public static async Task<TripDto> CreateTripAsync(this HttpClient client, string title = "Lisbon")
    {
        var response = await client.PostAsJsonAsync(
            "/api/trips",
            new TripRequest(title, null, "EUR", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 4)),
            Json);

        return await response.ReadAsync<TripDto>(HttpStatusCode.Created);
    }

    public static async Task<ParticipantDto> AddParticipantAsync(this HttpClient client, Guid tripId, string name)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/trips/{tripId}/participants",
            new ParticipantRequest(name, null),
            Json);

        return await response.ReadAsync<ParticipantDto>(HttpStatusCode.Created);
    }

    public static async Task<ExpenseDto> AddExpenseAsync(this HttpClient client, Guid tripId, ExpenseRequest request)
    {
        var response = await client.PostAsJsonAsync($"/api/trips/{tripId}/expenses", request, Json);
        return await response.ReadAsync<ExpenseDto>(HttpStatusCode.Created);
    }

    public static async Task<T> GetAsync<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(new Uri(url, UriKind.Relative));
        return await response.ReadAsync<T>(HttpStatusCode.OK);
    }

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expectedStatus, $"Expected {expectedStatus}, got {response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, Json)!;
    }
}
