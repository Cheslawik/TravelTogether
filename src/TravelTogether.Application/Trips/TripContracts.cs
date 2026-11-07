using TravelTogether.Application.Participants;

namespace TravelTogether.Application.Trips;

public sealed record TripRequest(
    string Title,
    string? Description,
    string Currency,
    DateOnly StartDate,
    DateOnly EndDate);

public sealed record TripSummaryDto(
    Guid Id,
    string Title,
    string Currency,
    DateOnly StartDate,
    DateOnly EndDate,
    int ParticipantCount,
    int ExpenseCount);

public sealed record TripDto(
    Guid Id,
    string Title,
    string? Description,
    string Currency,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalSpent,
    IReadOnlyList<ParticipantDto> Participants);
