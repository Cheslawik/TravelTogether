using TravelTogether.Domain.Common;
using TravelTogether.Domain.Splitting;

namespace TravelTogether.Domain.Entities;

/// <summary>Money paid by one participant on behalf of one or more participants.</summary>
public sealed class Expense
{
    public const int TitleMaxLength = 200;

    private readonly List<ExpenseSplit> _splits = [];

    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public Guid PaidByParticipantId { get; private set; }
    public string Title { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public SplitType SplitType { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<ExpenseSplit> Splits => _splits;

    private Expense()
    {
    }

    internal Expense(
        Guid tripId,
        Guid paidByParticipantId,
        string title,
        decimal amount,
        DateOnly date,
        SplitType splitType,
        IReadOnlyList<ExpenseShare> shares)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        CreatedAt = DateTime.UtcNow;
        Update(paidByParticipantId, title, amount, date, splitType, shares);
    }

    /// <summary>True if the participant paid for this expense or takes part in its split.</summary>
    public bool Involves(Guid participantId)
    {
        return PaidByParticipantId == participantId
            || _splits.Exists(s => s.ParticipantId == participantId);
    }

    internal void Update(
        Guid paidByParticipantId,
        string title,
        decimal amount,
        DateOnly date,
        SplitType splitType,
        IReadOnlyList<ExpenseShare> shares)
    {
        if (amount <= 0)
        {
            throw new DomainException("Expense amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new DomainException("Expense amount cannot have more than two decimal places.");
        }

        if (date == default)
        {
            throw new DomainException("Expense date is required.");
        }

        if (!Enum.IsDefined(splitType))
        {
            throw new DomainException("Unknown split type.");
        }

        var weights = splitType == SplitType.Equal
            ? shares.Select(_ => 1).ToArray()
            : shares.Select(s => s.Shares).ToArray();

        var amounts = ExpenseSplitter.Split(amount, weights);

        PaidByParticipantId = paidByParticipantId;
        Title = Guard.RequiredText(title, TitleMaxLength, "Expense title");
        Amount = amount;
        Date = date;
        SplitType = splitType;

        _splits.Clear();
        for (var i = 0; i < shares.Count; i++)
        {
            _splits.Add(new ExpenseSplit(Id, shares[i].ParticipantId, weights[i], amounts[i]));
        }
    }
}
