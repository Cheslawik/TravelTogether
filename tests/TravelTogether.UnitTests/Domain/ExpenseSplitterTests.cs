using TravelTogether.Domain.Common;
using TravelTogether.Domain.Splitting;

namespace TravelTogether.UnitTests.Domain;

public class ExpenseSplitterTests
{
    [Theory]
    [InlineData("100", new[] { 1, 1, 1 }, new[] { "33.34", "33.33", "33.33" })]
    [InlineData("10", new[] { 1, 1, 1 }, new[] { "3.34", "3.33", "3.33" })]
    [InlineData("0.05", new[] { 1, 1 }, new[] { "0.03", "0.02" })]
    [InlineData("100", new[] { 2, 1, 1 }, new[] { "50", "25", "25" })]
    [InlineData("100", new[] { 1, 2 }, new[] { "33.33", "66.67" })]
    [InlineData("10", new[] { 1, 1, 1, 3 }, new[] { "1.67", "1.67", "1.66", "5" })]
    public void Split_ShouldReturnExpectedParts(string amount, int[] weights, string[] expected)
    {
        var parts = ExpenseSplitter.Split(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture), weights);

        Assert.Equal(
            expected.Select(x => decimal.Parse(x, System.Globalization.CultureInfo.InvariantCulture)),
            parts);
    }

    [Fact]
    public void Split_PartsShouldAlwaysAddUpToAmount()
    {
        var random = new Random(42);

        for (var run = 0; run < 1_000; run++)
        {
            var amount = random.Next(100, 1_000_000) / 100m;
            var weights = Enumerable.Range(0, random.Next(1, 8)).Select(_ => random.Next(1, 5)).ToArray();

            var parts = ExpenseSplitter.Split(amount, weights);

            Assert.Equal(amount, parts.Sum());
        }
    }

    [Fact]
    public void Split_ShouldThrow_WhenShareIsNotPositive()
    {
        Assert.Throws<DomainException>(() => ExpenseSplitter.Split(10m, [1, 0]));
    }

    [Fact]
    public void Split_ShouldThrow_WhenSomeoneWouldGetZero()
    {
        Assert.Throws<DomainException>(() => ExpenseSplitter.Split(0.02m, [1, 1, 1]));
    }
}
