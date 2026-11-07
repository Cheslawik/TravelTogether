namespace TravelTogether.Domain.Entities;

/// <summary>The part of an expense a single participant is responsible for.</summary>
public sealed class ExpenseSplit
{
    public Guid Id { get; private set; }
    public Guid ExpenseId { get; private set; }
    public Guid ParticipantId { get; private set; }

    /// <summary>Weight used for <see cref="SplitType.Shares"/>; always 1 for an equal split.</summary>
    public int Shares { get; private set; }

    public decimal Amount { get; private set; }

    private ExpenseSplit()
    {
    }

    internal ExpenseSplit(Guid expenseId, Guid participantId, int shares, decimal amount)
    {
        Id = Guid.NewGuid();
        ExpenseId = expenseId;
        ParticipantId = participantId;
        Shares = shares;
        Amount = amount;
    }
}
