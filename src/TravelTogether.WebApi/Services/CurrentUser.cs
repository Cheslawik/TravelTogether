using Microsoft.IdentityModel.JsonWebTokens;
using TravelTogether.Application.Common.Abstractions;

namespace TravelTogether.WebApi.Services;

/// <summary>Reads the user id from the "sub" claim of the validated JWT.</summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var subject = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(subject, out var id)
                ? id
                : throw new InvalidOperationException("The request has no authenticated user.");
        }
    }
}
