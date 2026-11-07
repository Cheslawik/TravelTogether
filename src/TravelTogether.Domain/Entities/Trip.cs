using TravelTogether.Domain.Common;
using TravelTogether.Domain.Settlements;
using TravelTogether.Domain.Splitting;

namespace TravelTogether.Domain.Entities;

/// <summary>
/// Aggregate root: a trip with its participants and their shared expenses.
/// All changes to participants and expenses go through this class so that
/// its rules are always checked.
/// </summary>
public sealed class Trip
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 1000;
    public const int CurrencyLength = 3;

    private readonly List<Participant> _participants = [];
    private readonly List<Expense> _expenses = [];

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    /// <summary>ISO 4217 code of the currency every expense of the trip is recorded in, e.g. EUR.</summary>
    public string Currency { get; private set; } = null!;

    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Participant> Participants => _participants;
    public IReadOnlyCollection<Expense> Expenses => _expenses;

    private Trip()
    {
    }

    public Trip(
        string title,
        string? description,
        string currency,
        DateOnly startDate,
        DateOnly endDate)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Currency = NormalizeCurrency(currency);
        UpdateDetails(title, description, startDate, endDate);
    }

    public void UpdateDetails(
        string title,
        string? description,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (startDate == default || endDate == default)
        {
            throw new DomainException("Start and end dates are required.");
        }

        if (startDate > endDate)
        {
            throw new DomainException("Start date cannot be after end date.");
        }

        Title = Guard.RequiredText(title, TitleMaxLength, "Title");
        Description = Guard.OptionalText(description, DescriptionMaxLength, "Description");
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>The currency can only be changed while the trip has no expenses.</summary>
    public void ChangeCurrency(string currency)
    {
        var normalized = NormalizeCurrency(currency);

        if (normalized != Currency && _expenses.Count > 0)
        {
            throw new DomainException("The currency cannot be changed once the trip has expenses.");
        }

        Currency = normalized;
    }

    public Participant AddParticipant(string name, string? email)
    {
        EnsureParticipantIsUnique(name, email, exceptId: null);

        var participant = new Participant(Id, name, email);
        _participants.Add(participant);
        return participant;
    }

    public Participant UpdateParticipant(Guid participantId, string name, string? email)
    {
        var participant = GetParticipant(participantId);
        EnsureParticipantIsUnique(name, email, exceptId: participantId);

        participant.Update(name, email);
        return participant;
    }

    /// <summary>A participant who paid for or shares any expense cannot be removed.</summary>
    public void RemoveParticipant(Guid participantId)
    {
        var participant = GetParticipant(participantId);

        if (_expenses.Exists(e => e.Involves(participantId)))
        {
            throw new DomainException(
                "The participant is used in expenses. Remove or change those expenses first.");
        }

        _participants.Remove(participant);
    }

    /// <summary>Adds an expense split equally between the given participants.</summary>
    public Expense AddExpense(
        Guid paidByParticipantId,
        string title,
        decimal amount,
        DateOnly date,
        IReadOnlyCollection<Guid> splitParticipantIds)
    {
        ArgumentNullException.ThrowIfNull(splitParticipantIds);

        return AddExpense(
            paidByParticipantId,
            title,
            amount,
            date,
            SplitType.Equal,
            splitParticipantIds.Select(id => new ExpenseShare(id)).ToArray());
    }

    public Expense AddExpense(
        Guid paidByParticipantId,
        string title,
        decimal amount,
        DateOnly date,
        SplitType splitType,
        IReadOnlyCollection<ExpenseShare> shares)
    {
        var validShares = ValidateExpenseParticipants(paidByParticipantId, shares);

        var expense = new Expense(Id, paidByParticipantId, title, amount, date, splitType, validShares);

        _expenses.Add(expense);
        return expense;
    }

    public Expense UpdateExpense(
        Guid expenseId,
        Guid paidByParticipantId,
        string title,
        decimal amount,
        DateOnly date,
        SplitType splitType,
        IReadOnlyCollection<ExpenseShare> shares)
    {
        var expense = GetExpense(expenseId);
        var validShares = ValidateExpenseParticipants(paidByParticipantId, shares);

        expense.Update(paidByParticipantId, title, amount, date, splitType, validShares);
        return expense;
    }

    public void RemoveExpense(Guid expenseId)
    {
        _expenses.Remove(GetExpense(expenseId));
    }

    /// <summary>Paid amount, share of the costs and net balance of every participant.</summary>
    public IReadOnlyList<ParticipantBalance> CalculateBalances()
    {
        var splits = _expenses.SelectMany(e => e.Splits).ToArray();

        return _participants
            .Select(p => new ParticipantBalance(
                p.Id,
                Paid: _expenses.Where(e => e.PaidByParticipantId == p.Id).Sum(e => e.Amount),
                Share: splits.Where(s => s.ParticipantId == p.Id).Sum(s => s.Amount)))
            .ToArray();
    }

    /// <summary>The transfers that settle all debts of the trip, see <see cref="DebtSimplifier"/>.</summary>
    public IReadOnlyList<Transfer> CalculateSettlement()
    {
        return DebtSimplifier.Simplify(CalculateBalances());
    }

    public Participant GetParticipant(Guid participantId)
    {
        return _participants.Find(p => p.Id == participantId)
            ?? throw new DomainException("The participant is not part of this trip.");
    }

    public Expense GetExpense(Guid expenseId)
    {
        return _expenses.Find(e => e.Id == expenseId)
            ?? throw new DomainException("The expense does not belong to this trip.");
    }

    private ExpenseShare[] ValidateExpenseParticipants(
        Guid paidByParticipantId,
        IReadOnlyCollection<ExpenseShare> shares)
    {
        ArgumentNullException.ThrowIfNull(shares);

        if (!_participants.Exists(p => p.Id == paidByParticipantId))
        {
            throw new DomainException("The participant paying for the expense must be part of the trip.");
        }

        if (shares.Count == 0)
        {
            throw new DomainException("At least one participant must be included in the expense split.");
        }

        if (shares.DistinctBy(s => s.ParticipantId).Count() != shares.Count)
        {
            throw new DomainException("A participant cannot be included in an expense split more than once.");
        }

        if (shares.Any(s => !_participants.Exists(p => p.Id == s.ParticipantId)))
        {
            throw new DomainException("All split participants must be part of the trip.");
        }

        return shares.ToArray();
    }

    private void EnsureParticipantIsUnique(string name, string? email, Guid? exceptId)
    {
        name = Guard.RequiredText(name, Participant.NameMaxLength, "Participant name");
        email = Guard.OptionalText(email, Participant.EmailMaxLength, "Participant email");

        var others = _participants.Where(p => p.Id != exceptId).ToArray();

        if (others.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"A participant named '{name}' already exists in this trip.");
        }

        if (email is not null
            && others.Any(p => string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("A participant with this email already exists in this trip.");
        }
    }

    private static string NormalizeCurrency(string? currency)
    {
        var code = currency?.Trim().ToUpperInvariant();

        if (code is null || code.Length != CurrencyLength || !code.All(char.IsAsciiLetterUpper))
        {
            throw new DomainException("Currency must be a three-letter ISO 4217 code, e.g. EUR.");
        }

        return code;
    }
}
