using FluentValidation;
using TravelTogether.Application.Common;
using TravelTogether.Application.Common.Abstractions;
using TravelTogether.Domain.Splitting;

namespace TravelTogether.Application.Expenses;

public sealed class ExpenseService(
    ITripRepository trips,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IValidator<ExpenseRequest> validator)
{
    public async Task<IReadOnlyList<ExpenseDto>> ListAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);

        return trip.Expenses
            .OrderBy(e => e.Date)
            .ThenBy(e => e.CreatedAt)
            .Select(ExpenseDto.From)
            .ToArray();
    }

    public async Task<ExpenseDto> GetAsync(Guid tripId, Guid expenseId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        return ExpenseDto.From(trip.FindExpense(expenseId));
    }

    public async Task<ExpenseDto> AddAsync(Guid tripId, ExpenseRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);

        var expense = trip.AddExpense(
            request.PaidByParticipantId,
            request.Title,
            request.Amount,
            request.Date,
            request.SplitType,
            ToShares(request));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseDto.From(expense);
    }

    public async Task<ExpenseDto> UpdateAsync(
        Guid tripId,
        Guid expenseId,
        ExpenseRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        trip.FindExpense(expenseId);

        var expense = trip.UpdateExpense(
            expenseId,
            request.PaidByParticipantId,
            request.Title,
            request.Amount,
            request.Date,
            request.SplitType,
            ToShares(request));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseDto.From(expense);
    }

    public async Task DeleteAsync(Guid tripId, Guid expenseId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        trip.FindExpense(expenseId);

        trip.RemoveExpense(expenseId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ExpenseShare[] ToShares(ExpenseRequest request) =>
        request.Splits.Select(s => new ExpenseShare(s.ParticipantId, s.Shares ?? 1)).ToArray();
}
