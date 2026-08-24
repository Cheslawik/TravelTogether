namespace WebApplication1.Domain.Entities
{
    public class Expense
    {
        public Guid Id { get; private set; }
        public Guid TripId { get; private set; }
        public Guid PaidByParticipantId { get; private set; }
        public string Title { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Trip Trip { get; private set; } = null!;
        public Participant PaidBy { get; private set; } = null!;

        private readonly List<ExpenseSplit> _expenseSplits = [];
        public IReadOnlyCollection<ExpenseSplit> ExpenseSplits => _expenseSplits;
        
        private Expense() { }

        internal Expense(Guid tripId, Guid paidByParticipantId, string title, decimal amount, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title cannot be empty.", nameof(title));
            }

            if (tripId == Guid.Empty)
            {
                throw new ArgumentException("TripId must be a valid GUID.", nameof(tripId));
            }
            
            if (amount <= 0)
               { 
                    throw new ArgumentException("Amount must be greater than zero.", nameof(amount)); 
               }

            if (paidByParticipantId == Guid.Empty)
               { 
                    throw new ArgumentException("PaidByParticipantId must be a valid GUID.", nameof(paidByParticipantId)); 
               }

            if (date == default)
            {
                throw new ArgumentException("Date is required.", nameof(date));
            }

            if (decimal.Round(amount, 2) != amount)
            {
                throw new ArgumentException(
                    "Amount cannot have more than two decimal places.",
                    nameof(amount));
            }

            Id = Guid.NewGuid();
            TripId = tripId;
            PaidByParticipantId = paidByParticipantId;
            Title = title;
            Amount = amount;
            Date = date;
            CreatedAt = DateTime.UtcNow;
        }

        internal void SplitEqually(IReadOnlyList<Guid> participantIds)
        {
            ArgumentNullException.ThrowIfNull(participantIds);

            if (_expenseSplits.Count > 0)
            {
                throw new InvalidOperationException(
                    "Expense splits have already been created.");
            }

            if (participantIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one participant is required.",
                    nameof(participantIds));
            }

            if (Amount < participantIds.Count / 100m)
            {
                throw new InvalidOperationException(
                    "The expense amount is too small to split between all participants.");
            }           

            var baseAmount = Math.Floor(Amount / participantIds.Count * 100m) / 100m;
            var remainder = Amount - (baseAmount * participantIds.Count);

            for (var i = 0; i < participantIds.Count; i++)
            {
                var splitAmount = baseAmount;

                if (i == 0)
                {
                    splitAmount += remainder;
                }

                var split = new ExpenseSplit(
                    Id,
                    participantIds[i],
                    splitAmount);

                _expenseSplits.Add(split);
            }
        }
    }
}
