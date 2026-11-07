using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Common.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string normalizedEmail, CancellationToken cancellationToken);

    void Add(User user);
}
