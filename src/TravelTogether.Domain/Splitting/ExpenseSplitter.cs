using TravelTogether.Domain.Common;

namespace TravelTogether.Domain.Splitting;

/// <summary>
/// Divides an amount of money into parts proportional to integer weights, working in cents
/// so that the parts always add up exactly to the original amount.
/// </summary>
public static class ExpenseSplitter
{
    /// <summary>
    /// Splits <paramref name="amount"/> proportionally to <paramref name="weights"/>.
    /// Each part is first rounded down to a whole cent; the cents left over go one by one
    /// to the parts with the largest rounding loss (ties go to the earlier part).
    /// </summary>
    /// <example>100.00 with weights [1, 1, 1] gives [33.34, 33.33, 33.33].</example>
    public static IReadOnlyList<decimal> Split(decimal amount, IReadOnlyList<int> weights)
    {
        ArgumentNullException.ThrowIfNull(weights);

        if (weights.Count == 0)
        {
            throw new DomainException("At least one participant must be included in the expense split.");
        }

        if (weights.Any(w => w <= 0))
        {
            throw new DomainException("Every share must be a positive number.");
        }

        if (amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            throw new DomainException("Amount must be positive and have at most two decimal places.");
        }

        var totalCents = (long)(amount * 100m);
        long totalWeight = weights.Sum(w => (long)w);

        var cents = new long[weights.Count];
        var lostFractions = new long[weights.Count];

        for (var i = 0; i < weights.Count; i++)
        {
            var exact = totalCents * weights[i];
            cents[i] = exact / totalWeight;
            lostFractions[i] = exact % totalWeight;
        }

        var leftoverCents = totalCents - cents.Sum();

        var receiversOfLeftover = Enumerable.Range(0, weights.Count)
            .OrderByDescending(i => lostFractions[i])
            .ThenBy(i => i)
            .Take((int)leftoverCents);

        foreach (var i in receiversOfLeftover)
        {
            cents[i]++;
        }

        if (cents.Any(c => c == 0))
        {
            throw new DomainException("The expense amount is too small to split between all participants.");
        }

        return cents.Select(c => c / 100m).ToArray();
    }
}
