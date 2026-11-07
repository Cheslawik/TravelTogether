using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Expenses;

/// <param name="Title">What the money was spent on.</param>
/// <param name="Amount">Total amount in the trip currency, at most two decimal places.</param>
/// <param name="Date">Day of the expense.</param>
/// <param name="PaidByParticipantId">Participant who paid.</param>
/// <param name="SplitType">Equal or Shares.</param>
/// <param name="Splits">Participants sharing the expense; Shares is required for SplitType.Shares.</param>
public sealed record ExpenseRequest(
    string Title,
    decimal Amount,
    DateOnly Date,
    Guid PaidByParticipantId,
    SplitType SplitType,
    IReadOnlyList<ExpenseShareRequest> Splits);

public sealed record ExpenseShareRequest(Guid ParticipantId, int? Shares = null);

public sealed record ExpenseDto(
    Guid Id,
    string Title,
    decimal Amount,
    DateOnly Date,
    Guid PaidByParticipantId,
    SplitType SplitType,
    IReadOnlyList<ExpenseSplitDto> Splits)
{
    public static ExpenseDto From(Expense expense) => new(
        expense.Id,
        expense.Title,
        expense.Amount,
        expense.Date,
        expense.PaidByParticipantId,
        expense.SplitType,
        expense.Splits.Select(s => new ExpenseSplitDto(s.ParticipantId, s.Shares, s.Amount)).ToArray());
}

public sealed record ExpenseSplitDto(Guid ParticipantId, int Shares, decimal Amount);
