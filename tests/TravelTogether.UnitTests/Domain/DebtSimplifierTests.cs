using TravelTogether.Domain.Settlements;

namespace TravelTogether.UnitTests.Domain;

public class DebtSimplifierTests
{
    private static readonly Guid Alice = new("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid Bob = new("00000000-0000-0000-0000-00000000000b");
    private static readonly Guid Chris = new("00000000-0000-0000-0000-00000000000c");
    private static readonly Guid Dana = new("00000000-0000-0000-0000-00000000000d");

    private static ParticipantBalance Net(Guid id, decimal net) =>
        net >= 0 ? new(id, net, 0) : new(id, 0, -net);

    [Fact]
    public void Simplify_ShouldReturnNothing_WhenEveryoneIsSettled()
    {
        var transfers = DebtSimplifier.Simplify([Net(Alice, 0), Net(Bob, 0)]);

        Assert.Empty(transfers);
    }

    [Fact]
    public void Simplify_ShouldLetEachDebtorPayTheOnlyCreditor()
    {
        // Alice paid 90 for a dinner of three: Bob and Chris owe her 30 each.
        var transfers = DebtSimplifier.Simplify([Net(Alice, 60), Net(Bob, -30), Net(Chris, -30)]);

        Assert.Equal(2, transfers.Count);
        Assert.Contains(new Transfer(Bob, Alice, 30), transfers);
        Assert.Contains(new Transfer(Chris, Alice, 30), transfers);
    }

    [Fact]
    public void Simplify_ShouldRerouteChainOfDebts()
    {
        // Alice owes Bob 10 and Bob owes Chris 10: Bob's net is zero, Alice pays Chris directly.
        var transfers = DebtSimplifier.Simplify([Net(Alice, -10), Net(Bob, 0), Net(Chris, 10)]);

        Assert.Equal([new Transfer(Alice, Chris, 10)], transfers);
    }

    [Fact]
    public void Simplify_ShouldPairUpExactlyMatchingAmountsFirst()
    {
        var transfers = DebtSimplifier.Simplify(
            [Net(Alice, -25.50m), Net(Bob, -4.50m), Net(Chris, 4.50m), Net(Dana, 25.50m)]);

        Assert.Equal(2, transfers.Count);
        Assert.Contains(new Transfer(Alice, Dana, 25.50m), transfers);
        Assert.Contains(new Transfer(Bob, Chris, 4.50m), transfers);
    }

    [Fact]
    public void Simplify_ShouldSplitLargestDebtBetweenCreditors()
    {
        var transfers = DebtSimplifier.Simplify([Net(Alice, -100), Net(Bob, 70), Net(Chris, 30)]);

        Assert.Equal(
            [new Transfer(Alice, Bob, 70), new Transfer(Alice, Chris, 30)],
            transfers);
    }

    [Fact]
    public void Simplify_ShouldThrow_WhenBalancesDoNotAddUpToZero()
    {
        Assert.Throws<InvalidOperationException>(() => DebtSimplifier.Simplify([Net(Alice, 10), Net(Bob, -5)]));
    }

    [Fact]
    public void Simplify_ShouldSettleEveryone_WithAtMostNMinusOneTransfers()
    {
        var random = new Random(2026);

        for (var run = 0; run < 500; run++)
        {
            var balances = RandomBalances(random, people: random.Next(2, 12));

            var transfers = DebtSimplifier.Simplify(balances);

            var remaining = balances.ToDictionary(b => b.ParticipantId, b => b.Net);
            foreach (var t in transfers)
            {
                Assert.True(t.Amount > 0);
                Assert.True(remaining[t.FromParticipantId] < 0, "Only debtors pay.");
                Assert.True(remaining[t.ToParticipantId] > 0, "Only creditors receive.");

                remaining[t.FromParticipantId] += t.Amount;
                remaining[t.ToParticipantId] -= t.Amount;
            }

            Assert.All(remaining.Values, net => Assert.Equal(0, net));

            var nonZero = balances.Count(b => b.Net != 0);
            Assert.True(transfers.Count <= Math.Max(0, nonZero - 1));
        }
    }

    private static List<ParticipantBalance> RandomBalances(Random random, int people)
    {
        var nets = Enumerable.Range(0, people - 1)
            .Select(_ => random.Next(-50_000, 50_000) / 100m)
            .ToList();
        nets.Add(-nets.Sum());

        return nets.Select(n => Net(Guid.NewGuid(), n)).ToList();
    }
}
