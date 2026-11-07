using System.Net;
using System.Net.Http.Json;
using TravelTogether.Application.Expenses;
using TravelTogether.Application.Participants;
using TravelTogether.Application.Settlements;
using TravelTogether.Application.Trips;
using TravelTogether.Domain.Entities;

namespace TravelTogether.IntegrationTests;

public class TripFlowTests(TravelTogetherApiFactory factory) : IClassFixture<TravelTogetherApiFactory>
{
    private static readonly DateOnly Day = new(2026, 8, 1);

    [Fact]
    public async Task FullTrip_ShouldCalculateBalancesAndSettlement()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");
        var ben = await client.AddParticipantAsync(trip.Id, "Ben");
        var carl = await client.AddParticipantAsync(trip.Id, "Carl");

        await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Apartment", 300m, Day, anna.Id, SplitType.Shares,
            [new(anna.Id, 2), new(ben.Id, 1), new(carl.Id, 1)]));
        await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Dinner", 90m, Day, ben.Id, SplitType.Equal,
            [new(anna.Id), new(ben.Id), new(carl.Id)]));
        await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Taxi", 20m, Day, carl.Id, SplitType.Equal,
            [new(ben.Id), new(carl.Id)]));

        var balances = await client.GetAsync<BalancesDto>($"/api/trips/{trip.Id}/balances");
        var settlement = await client.GetAsync<SettlementDto>($"/api/trips/{trip.Id}/settlement");
        var details = await client.GetAsync<TripDto>($"/api/trips/{trip.Id}");

        Assert.Equal("EUR", balances.Currency);
        Assert.Equal(
            [("Anna", 120m), ("Ben", -25m), ("Carl", -95m)],
            balances.Balances.Select(b => (b.Name, b.Net)));

        Assert.Equal(410m, details.TotalSpent);
        Assert.Equal(2, settlement.Transfers.Count);
        Assert.Contains(settlement.Transfers, t => t is { FromName: "Carl", ToName: "Anna", Amount: 95m });
        Assert.Contains(settlement.Transfers, t => t is { FromName: "Ben", ToName: "Anna", Amount: 25m });
    }

    [Fact]
    public async Task UpdateExpense_ShouldReplaceSplitsAndChangeSettlement()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");
        var ben = await client.AddParticipantAsync(trip.Id, "Ben");
        var expense = await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Dinner", 50m, Day, anna.Id, SplitType.Equal, [new(anna.Id), new(ben.Id)]));

        var response = await client.PutAsJsonAsync(
            $"/api/trips/{trip.Id}/expenses/{expense.Id}",
            new ExpenseRequest("Dinner", 90m, Day, anna.Id, SplitType.Shares, [new(anna.Id, 1), new(ben.Id, 2)]),
            ApiClientExtensions.Json);
        await response.ReadAsync<ExpenseDto>(HttpStatusCode.OK);

        var settlement = await client.GetAsync<SettlementDto>($"/api/trips/{trip.Id}/settlement");

        var transfer = Assert.Single(settlement.Transfers);
        Assert.Equal(("Ben", "Anna", 60m), (transfer.FromName, transfer.ToName, transfer.Amount));
    }

    [Fact]
    public async Task DeleteExpense_ShouldClearDebts()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");
        var ben = await client.AddParticipantAsync(trip.Id, "Ben");
        var expense = await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Dinner", 50m, Day, anna.Id, SplitType.Equal, [new(anna.Id), new(ben.Id)]));

        var response = await client.DeleteAsync(new Uri($"/api/trips/{trip.Id}/expenses/{expense.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await client.GetAsync<SettlementDto>($"/api/trips/{trip.Id}/settlement")).Transfers);
    }

    [Fact]
    public async Task CreateTrip_ShouldReturnValidationProblem_WhenCurrencyIsInvalid()
    {
        var client = await factory.AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/trips", new TripRequest("Lisbon", null, "EURO", Day, Day));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Currency", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddExpense_ShouldReturnUnprocessable_WhenPayerIsNotInTrip()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");

        var response = await client.PostAsJsonAsync(
            $"/api/trips/{trip.Id}/expenses",
            new ExpenseRequest("Dinner", 10m, Day, Guid.NewGuid(), SplitType.Equal, [new(anna.Id)]),
            ApiClientExtensions.Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task RemoveParticipant_ShouldReturnUnprocessable_WhenUsedInExpense()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");
        await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Dinner", 10m, Day, anna.Id, SplitType.Equal, [new(anna.Id)]));

        var response = await client.DeleteAsync(new Uri($"/api/trips/{trip.Id}/participants/{anna.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UpdateParticipant_ShouldRename()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");

        var response = await client.PutAsJsonAsync(
            $"/api/trips/{trip.Id}/participants/{anna.Id}",
            new ParticipantRequest("Anna K.", "anna@example.com"));
        var updated = await response.ReadAsync<ParticipantDto>(HttpStatusCode.OK);

        Assert.Equal("Anna K.", updated.Name);
    }

    [Fact]
    public async Task TripOfAnotherUser_ShouldNotBeVisible()
    {
        var owner = await factory.AuthenticatedClientAsync();
        var stranger = await factory.AuthenticatedClientAsync();
        var trip = await owner.CreateTripAsync();

        var response = await stranger.GetAsync(new Uri($"/api/trips/{trip.Id}", UriKind.Relative));
        var strangersTrips = await stranger.GetAsync<List<TripSummaryDto>>("/api/trips");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(strangersTrips);
    }

    [Fact]
    public async Task DeleteTrip_ShouldRemoveTripWithParticipantsAndExpenses()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync();
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");
        await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Dinner", 10m, Day, anna.Id, SplitType.Equal, [new(anna.Id)]));

        var delete = await client.DeleteAsync(new Uri($"/api/trips/{trip.Id}", UriKind.Relative));
        var get = await client.GetAsync(new Uri($"/api/trips/{trip.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task ListTrips_ShouldReturnCounts()
    {
        var client = await factory.AuthenticatedClientAsync();
        var trip = await client.CreateTripAsync("Porto");
        var anna = await client.AddParticipantAsync(trip.Id, "Anna");
        await client.AddParticipantAsync(trip.Id, "Ben");
        await client.AddExpenseAsync(trip.Id, new ExpenseRequest(
            "Dinner", 10m, Day, anna.Id, SplitType.Equal, [new(anna.Id)]));

        var trips = await client.GetAsync<List<TripSummaryDto>>("/api/trips");

        var summary = Assert.Single(trips);
        Assert.Equal(("Porto", 2, 1), (summary.Title, summary.ParticipantCount, summary.ExpenseCount));
    }
}
