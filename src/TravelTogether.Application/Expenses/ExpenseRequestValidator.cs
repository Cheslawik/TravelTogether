using FluentValidation;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Expenses;

public sealed class ExpenseRequestValidator : AbstractValidator<ExpenseRequest>
{
    public const int MaxShares = 1000;

    public ExpenseRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Expense.TitleMaxLength);
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.PaidByParticipantId).NotEmpty();
        RuleFor(x => x.SplitType).IsInEnum();

        RuleFor(x => x.Splits)
            .NotEmpty().WithMessage("At least one participant must be included in the split.")
            .Must(splits => splits.DistinctBy(s => s.ParticipantId).Count() == splits.Count)
            .WithMessage("A participant cannot be included in the split more than once.");

        RuleForEach(x => x.Splits).ChildRules(split =>
        {
            split.RuleFor(s => s.ParticipantId).NotEmpty();
            split.RuleFor(s => s.Shares).InclusiveBetween(1, MaxShares);
        });

        RuleForEach(x => x.Splits)
            .Must(s => s.Shares is not null)
            .When(x => x.SplitType == SplitType.Shares)
            .WithMessage("Shares are required for every participant when the split type is Shares.");
    }
}
