namespace TravelTogether.Application.Common.Exceptions;

/// <summary>The requested resource does not exist or does not belong to the current user.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public static NotFoundException For(string resource, Guid id) => new($"{resource} '{id}' was not found.");
}
