using Microsoft.EntityFrameworkCore;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(TravelTogetherDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public Task<bool> ExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

    public void Add(User user) => db.Users.Add(user);
}
