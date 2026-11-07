using System.Net;
using System.Net.Http.Json;
using TravelTogether.Application.Auth;

namespace TravelTogether.IntegrationTests;

public class AuthTests(TravelTogetherApiFactory factory) : IClassFixture<TravelTogetherApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ThenLogin_ShouldReturnAccessToken()
    {
        var email = $"{Guid.NewGuid():N}@Example.com";
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email.ToUpperInvariant(), "Passw0rd!"));
        var auth = await response.ReadAsync<AuthResponse>(HttpStatusCode.OK);

        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.Equal(email.ToLowerInvariant(), auth.Email);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailIsTaken()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Passw0rd!", "Again"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturnValidationErrors_WhenPasswordIsWeak()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("weak@example.com", "short", "Weak"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Password", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassw0rd"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Trips_ShouldRequireToken()
    {
        var response = await _client.GetAsync(new Uri("/api/trips", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_ShouldBeAnonymous()
    {
        var response = await _client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_ShouldDescribeTheApi()
    {
        var json = await _client.GetStringAsync(new Uri("/swagger/v1/swagger.json", UriKind.Relative));

        Assert.Contains("/api/trips/{tripId}/settlement", json, StringComparison.Ordinal);
    }
}
