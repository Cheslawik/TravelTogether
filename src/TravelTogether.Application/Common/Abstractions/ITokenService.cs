using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Common.Abstractions;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);
