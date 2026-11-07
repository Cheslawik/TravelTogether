namespace TravelTogether.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "TravelTogether";

    public string Audience { get; init; } = "TravelTogether";

    /// <summary>HMAC-SHA256 key, at least 32 characters. Never commit a production key.</summary>
    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 60;
}
