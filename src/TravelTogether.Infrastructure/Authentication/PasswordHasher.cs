using Microsoft.AspNetCore.Identity;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Authentication;

/// <summary>Uses ASP.NET Core Identity's PBKDF2 hasher (salted, 100 000 iterations, HMAC-SHA512).</summary>
internal sealed class PasswordHasher : Application.Common.Abstractions.IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
