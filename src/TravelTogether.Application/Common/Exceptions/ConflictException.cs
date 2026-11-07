namespace TravelTogether.Application.Common.Exceptions;

/// <summary>The request conflicts with the current state, e.g. an email that is already registered.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
