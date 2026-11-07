using TravelTogether.Domain.Entities;
using TravelTogether.Domain.Settlements;
using TravelTogether.Domain.Splitting;

namespace TravelTogether.UnitTests.Domain;

public class TripBalanceTests
{
    private static readonly DateOnly Day = new(2026, 8, 2);

    [Fact]
    public void CalculateBalances_ShouldSumPaidAndShareOfEachParticipant()
    {
        var trip = new Trip(Guid.NewGuid(), "Lisbon", null, "EUR", Day, Day.AddDays(3));
        var anna = trip.AddParticipant("Anna", null);
        var ben = trip.AddParticipant("Ben", null);
        var carl = trip.AddParticipant("Carl", null);

        // Anna pays the apartment, split 2:1:1 because she had a room alone.
        trip.AddExpense(anna.Id, "Apartment", 300m, Day, SplitType.Shares,
            [new ExpenseShare(anna.Id, 2), new ExpenseShare(ben.Id, 1), new ExpenseShare(carl.Id, 1)]);
        // Ben pays dinner for everyone, split equally.
        trip.AddExpense(ben.Id, "Dinner", 90m, Day, [anna.Id, ben.Id, carl.Id]);
        // Carl pays a taxi for himself and Ben.
        trip.AddExpense(carl.Id, "Taxi", 20m, Day, [ben.Id, carl.Id]);

        var balances = trip.CalculateBalances().ToDictionary(b => b.ParticipantId);

        Assert.Equal(new ParticipantBalance(anna.Id, Paid: 300m, Share: 180m), balances[anna.Id]);
        Assert.Equal(new ParticipantBalance(ben.Id, Paid: 90m, Share: 115m), balances[ben.Id]);
        Assert.Equal(new ParticipantBalance(carl.Id, Paid: 20m, Share: 115m), balances[carl.Id]);
        Assert.Equal(0m, balances.Values.Sum(b => b.Net));
    }

    [Fact]
    public void CalculateSettlement_ShouldSettleAllDebtsOfTrip()
    {
        var trip = new Trip(Guid.NewGuid(), "Lisbon", null, "EUR", Day, Day.AddDays(3));
        var anna = trip.AddParticipant("Anna", null);
        var ben = trip.AddParticipant("Ben", null);
        var carl = trip.AddParticipant("Carl", null);

        trip.AddExpense(anna.Id, "Apartment", 300m, Day, SplitType.Shares,
            [new ExpenseShare(anna.Id, 2), new ExpenseShare(ben.Id, 1), new ExpenseShare(carl.Id, 1)]);
        trip.AddExpense(ben.Id, "Dinner", 90m, Day, [anna.Id, ben.Id, carl.Id]);
        trip.AddExpense(carl.Id, "Taxi", 20m, Day, [ben.Id, carl.Id]);

        var transfers = trip.CalculateSettlement();

        // Anna +120, Ben -25, Carl -95.
        Assert.Equal(2, transfers.Count);
        Assert.Contains(new Transfer(carl.Id, anna.Id, 95m), transfers);
        Assert.Contains(new Transfer(ben.Id, anna.Id, 25m), transfers);
    }

    [Fact]
    public void CalculateSettlement_ShouldBeEmpty_WhenTripHasNoExpenses()
    {
        var trip = new Trip(Guid.NewGuid(), "Lisbon", null, "EUR", Day, Day);
        trip.AddParticipant("Anna", null);

        Assert.Empty(trip.CalculateSettlement());
    }
}
