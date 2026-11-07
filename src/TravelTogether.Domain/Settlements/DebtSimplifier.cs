namespace TravelTogether.Domain.Settlements;

/// <summary>
/// Turns net balances into a short list of transfers that settles everyone.
/// </summary>
/// <remarks>
/// Only the net balance of each person matters, not who paid for whom, so debts can be
/// "rerouted": if Alice owes Bob 10 and Bob owes Chris 10, Alice simply pays Chris 10.
///
/// Algorithm:
/// 1. Pair up a debtor and a creditor whose amounts are exactly equal - one transfer
///    settles both of them.
/// 2. Greedy: repeatedly let the person who owes the most pay the person who is owed
///    the most, as much as possible. Each step settles at least one person, so the
///    result has at most N - 1 transfers for N people with a non-zero balance.
///
/// Finding the absolute minimum number of transfers is NP-hard in general (it requires
/// splitting people into the largest number of groups whose balances sum to zero);
/// this heuristic gives the optimum or close to it for realistic group sizes.
/// </remarks>
public static class DebtSimplifier
{
    public static IReadOnlyList<Transfer> Simplify(IEnumerable<ParticipantBalance> balances)
    {
        ArgumentNullException.ThrowIfNull(balances);

        var people = balances
            .Where(b => b.Net != 0)
            .Select(b => new Person(b.ParticipantId, b.Net))
            .ToList();

        if (people.Sum(p => p.Net) != 0)
        {
            throw new InvalidOperationException("Balances must add up to zero.");
        }

        var transfers = new List<Transfer>();

        SettleExactMatches(people, transfers);
        SettleGreedily(people, transfers);

        return transfers;
    }

    private static void SettleExactMatches(List<Person> people, List<Transfer> transfers)
    {
        foreach (var debtor in people.Where(p => p.Net < 0).OrderBy(p => p.Net))
        {
            var creditor = people.Find(p => p.Net == -debtor.Net && p.Net > 0);

            if (creditor is not null)
            {
                Pay(debtor, creditor, creditor.Net, transfers);
            }
        }

        people.RemoveAll(p => p.Net == 0);
    }

    private static void SettleGreedily(List<Person> people, List<Transfer> transfers)
    {
        while (people.Count > 0)
        {
            // Ties are broken by id so the result is deterministic.
            var debtor = people.Where(p => p.Net < 0).OrderBy(p => p.Net).ThenBy(p => p.Id).First();
            var creditor = people.Where(p => p.Net > 0).OrderByDescending(p => p.Net).ThenBy(p => p.Id).First();

            Pay(debtor, creditor, Math.Min(-debtor.Net, creditor.Net), transfers);

            people.RemoveAll(p => p.Net == 0);
        }
    }

    private static void Pay(Person debtor, Person creditor, decimal amount, List<Transfer> transfers)
    {
        debtor.Net += amount;
        creditor.Net -= amount;
        transfers.Add(new Transfer(debtor.Id, creditor.Id, amount));
    }

    private sealed class Person(Guid id, decimal net)
    {
        public Guid Id { get; } = id;
        public decimal Net { get; set; } = net;
    }
}
