using TravelTogether.Domain.Common;
using TravelTogether.Domain.Entities;
using TravelTogether.Domain.Splitting;

namespace TravelTogether.UnitTests.Domain;

public class TripTests
{
    private static readonly DateOnly Day = new(2026, 8, 2);

    private static Trip CreateTrip() =>
        new("Italy", null, "EUR", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10));

    [Fact]
    public void Constructor_ShouldThrow_WhenStartDateIsAfterEndDate()
    {
        var action = () => new Trip("Italy", null, "EUR", new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 1));

        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    public void Constructor_ShouldThrow_WhenCurrencyIsNotIsoCode(string currency)
    {
        var action = () => new Trip("Italy", null, currency, Day, Day);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrencyToUpperCase()
    {
        var trip = new Trip("Italy", null, "pln", Day, Day);

        Assert.Equal("PLN", trip.Currency);
    }

    [Fact]
    public void AddExpense_ShouldSplitAmountEqually()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");
        var bob = trip.AddParticipant("Bob", "bob@example.com");
        var john = trip.AddParticipant("John", "john@example.com");

        var expense = trip.AddExpense(alice.Id, "Dinner", 100m, Day, [alice.Id, bob.Id, john.Id]);

        Assert.Equal(SplitType.Equal, expense.SplitType);
        Assert.Equal(100m, expense.Splits.Sum(x => x.Amount));
        Assert.Equal([33.34m, 33.33m, 33.33m], expense.Splits.Select(x => x.Amount));
    }

    [Fact]
    public void AddExpense_ShouldSplitAmountByShares()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);
        var bob = trip.AddParticipant("Bob", null);
        var john = trip.AddParticipant("John", null);

        var expense = trip.AddExpense(
            alice.Id,
            "Apartment",
            400m,
            Day,
            SplitType.Shares,
            [new ExpenseShare(alice.Id, 2), new ExpenseShare(bob.Id, 1), new ExpenseShare(john.Id, 1)]);

        Assert.Equal([200m, 100m, 100m], expense.Splits.Select(x => x.Amount));
        Assert.Equal([2, 1, 1], expense.Splits.Select(x => x.Shares));
    }

    [Fact]
    public void AddExpense_ShouldIgnoreShares_WhenSplitIsEqual()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);
        var bob = trip.AddParticipant("Bob", null);

        var expense = trip.AddExpense(
            alice.Id, "Taxi", 30m, Day, SplitType.Equal,
            [new ExpenseShare(alice.Id, 5), new ExpenseShare(bob.Id, 1)]);

        Assert.Equal([15m, 15m], expense.Splits.Select(x => x.Amount));
        Assert.All(expense.Splits, s => Assert.Equal(1, s.Shares));
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenSplitParticipantsAreEmpty()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddExpense(alice.Id, "Dinner", 100m, Day, []);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenParticipantIsDuplicated()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddExpense(alice.Id, "Dinner", 100m, Day, [alice.Id, alice.Id]);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenPayerDoesNotBelongToTrip()
    {
        var trip = CreateTrip();

        var action = () => trip.AddExpense(Guid.NewGuid(), "Dinner", 100m, Day, [Guid.NewGuid()]);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenSplitParticipantDoesNotBelongToTrip()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddExpense(alice.Id, "Dinner", 100m, Day, [Guid.NewGuid()]);

        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(10.001)]
    public void AddExpense_ShouldThrow_WhenAmountIsInvalid(decimal amount)
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);

        var action = () => trip.AddExpense(alice.Id, "Dinner", amount, Day, [alice.Id]);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenAmountTooSmallForParticipants()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");
        var bob = trip.AddParticipant("Bob", "bob@example.com");

        var action = () => trip.AddExpense(alice.Id, "Small expense", 0.01m, Day, [alice.Id, bob.Id]);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddParticipant_ShouldThrow_WhenEmailAlreadyExists()
    {
        var trip = CreateTrip();
        trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddParticipant("Another Alice", "ALICE@example.com");

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddParticipant_ShouldThrow_WhenNameAlreadyExists()
    {
        var trip = CreateTrip();
        trip.AddParticipant("Alice", null);

        var action = () => trip.AddParticipant(" alice ", null);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdateParticipant_ShouldAllowKeepingOwnName()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);

        trip.UpdateParticipant(alice.Id, "Alice", "alice@example.com");

        Assert.Equal("alice@example.com", alice.Email);
    }

    [Fact]
    public void RemoveParticipant_ShouldThrow_WhenParticipantIsUsedInExpense()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);
        var bob = trip.AddParticipant("Bob", null);
        trip.AddExpense(alice.Id, "Dinner", 50m, Day, [alice.Id, bob.Id]);

        var action = () => trip.RemoveParticipant(bob.Id);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void RemoveParticipant_ShouldRemove_WhenParticipantHasNoExpenses()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);

        trip.RemoveParticipant(alice.Id);

        Assert.Empty(trip.Participants);
    }

    [Fact]
    public void UpdateExpense_ShouldReplaceSplits()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);
        var bob = trip.AddParticipant("Bob", null);
        var expense = trip.AddExpense(alice.Id, "Dinner", 50m, Day, [alice.Id, bob.Id]);

        trip.UpdateExpense(
            expense.Id, bob.Id, "Dinner and wine", 90m, Day, SplitType.Shares,
            [new ExpenseShare(alice.Id, 1), new ExpenseShare(bob.Id, 2)]);

        Assert.Equal(bob.Id, expense.PaidByParticipantId);
        Assert.Equal([30m, 60m], expense.Splits.Select(x => x.Amount));
    }

    [Fact]
    public void ChangeCurrency_ShouldThrow_WhenTripHasExpenses()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", null);
        trip.AddExpense(alice.Id, "Dinner", 50m, Day, [alice.Id]);

        var action = () => trip.ChangeCurrency("PLN");

        Assert.Throws<DomainException>(action);
    }
}
