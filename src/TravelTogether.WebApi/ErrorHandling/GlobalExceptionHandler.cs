using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravelTogether.Application.Common.Exceptions;
using TravelTogether.Domain.Common;

namespace TravelTogether.WebApi.ErrorHandling;

/// <summary>
/// Turns exceptions into RFC 9457 problem details responses, so every error
/// has the same JSON shape and no stack trace leaks to the client.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(
                validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            },
            NotFoundException => Problem(StatusCodes.Status404NotFound, "Not found", exception.Message),
            ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message),
            InvalidCredentialsException => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            DomainException => Problem(StatusCodes.Status422UnprocessableEntity, "Business rule violated", exception.Message),
            _ => null,
        };

        if (problem is null)
        {
            LogUnhandledException(logger, exception);
            problem = Problem(StatusCodes.Status500InternalServerError, "An unexpected error occurred.", detail: null);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails Problem(int status, string title, string? detail) =>
        new() { Status = status, Title = title, Detail = detail };

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
