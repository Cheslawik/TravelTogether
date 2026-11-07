using FluentValidation;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Application.Common.Exceptions;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = User.NormalizeEmail(request.Email);

        if (await users.ExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("A user with this email is already registered.");
        }

        var user = new User(email, request.DisplayName, passwordHasher.Hash(request.Password));

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken);

        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var token = tokenService.CreateAccessToken(user);
        return new AuthResponse(user.Id, user.Email, user.DisplayName, token.Token, token.ExpiresAt);
    }
}
