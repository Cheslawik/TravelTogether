using WebApplication1.Domain.Entities;

namespace WebApplication1.Domain.Tests;

public class TripTests
{
    private static Trip CreateTrip()
    {
        return new Trip(
            "Italy",
            null,
            new DateTime(2026, 8, 1),
            new DateTime(2026, 8, 10));
    }

    [Fact]
    public void AddExpense_ShouldSplitAmountEqually()
    {
        var trip = CreateTrip();

        var alice = trip.AddParticipant("Alice", "alice@example.com");
        var bob = trip.AddParticipant("Bob", "bob@example.com");
        var john = trip.AddParticipant("John", "john@example.com");

        var expense = trip.AddExpense(
            alice.Id,
            "Dinner",
            100m,
            new DateTime(2026, 8, 2),
            [alice.Id, bob.Id, john.Id]);

        Assert.Equal(3, expense.ExpenseSplits.Count);
        Assert.Equal(100m, expense.ExpenseSplits.Sum(x => x.Amount));

        Assert.Equal(33.34m, expense.ExpenseSplits.ElementAt(0).Amount);
        Assert.Equal(33.33m, expense.ExpenseSplits.ElementAt(1).Amount);
        Assert.Equal(33.33m, expense.ExpenseSplits.ElementAt(2).Amount);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenSplitParticipantsAreEmpty()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddExpense(
            alice.Id,
            "Dinner",
            100m,
            DateTime.UtcNow,
            []);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenParticipantIsDuplicated()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddExpense(
            alice.Id,
            "Dinner",
            100m,
            DateTime.UtcNow,
            [alice.Id, alice.Id]);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenPayerDoesNotBelongToTrip()
    {
        var trip = CreateTrip();

        var action = () => trip.AddExpense(
            Guid.NewGuid(),
            "Dinner",
            100m,
            DateTime.UtcNow,
            [Guid.NewGuid()]);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenSplitParticipantDoesNotBelongToTrip()
    {
        var trip = CreateTrip();
        var alice = trip.AddParticipant("Alice", "alice@example.com");

        var action = () => trip.AddExpense(
            alice.Id,
            "Dinner",
            100m,
            DateTime.UtcNow,
            [Guid.NewGuid()]);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddParticipant_ShouldThrow_WhenEmailAlreadyExists()
    {
        var trip = CreateTrip();

        trip.AddParticipant("Alice", "alice@example.com");

        var action = () =>
            trip.AddParticipant("Another Alice", "ALICE@example.com");

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddExpense_ShouldThrow_WhenAmountTooSmallForParticipants()
    {
        var trip = CreateTrip();

        var alice = trip.AddParticipant("Alice", "alice@example.com");
        var bob = trip.AddParticipant("Bob", "bob@example.com");

        var action = () => trip.AddExpense(
            alice.Id,
            "Small expense",
            0.01m,
            DateTime.UtcNow,
            [alice.Id,  bob.Id]);

        Assert.Throws<InvalidOperationException>(action);
    }
}
