using TravelTogether.Application.Expenses;
using TravelTogether.Domain.Entities;

namespace TravelTogether.UnitTests.Application;

public class ExpenseRequestValidatorTests
{
    private static readonly Guid Alice = Guid.NewGuid();
    private static readonly Guid Bob = Guid.NewGuid();

    private readonly ExpenseRequestValidator _validator = new();

    private static ExpenseRequest Valid() => new(
        "Dinner", 50m, new DateOnly(2026, 8, 2), Alice, SplitType.Equal,
        [new ExpenseShareRequest(Alice), new ExpenseShareRequest(Bob)]);

    [Fact]
    public void Validate_ShouldPass_ForValidEqualSplit()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountHasMoreThanTwoDecimals()
    {
        var result = _validator.Validate(Valid() with { Amount = 10.005m });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseRequest.Amount));
    }

    [Fact]
    public void Validate_ShouldFail_WhenSharesMissingForSharesSplit()
    {
        var result = _validator.Validate(Valid() with { SplitType = SplitType.Shares });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenParticipantIsRepeated()
    {
        var result = _validator.Validate(Valid() with
        {
            Splits = [new ExpenseShareRequest(Alice), new ExpenseShareRequest(Alice)],
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenSplitTypeIsUnknown()
    {
        var result = _validator.Validate(Valid() with { SplitType = (SplitType)42 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExpenseRequest.SplitType));
    }
}
