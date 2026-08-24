namespace WebApplication1.Domain.Entities
{
    public class ExpenseSplit
    {
        public Guid Id { get; private set; }
        public Guid ExpenseId { get; private set; }
        public Guid ParticipantId { get; private set; }
        public decimal Amount { get; private set; }

        public Expense Expense { get; private set; } = null!;
        public Participant Participant { get; private set; } = null!;

        private ExpenseSplit() { }

        internal ExpenseSplit(Guid expenseId, Guid participantId, decimal amount)
        {
            if (expenseId == Guid.Empty)
            {
                throw new ArgumentException("ExpenseId must be a valid GUID.", nameof(expenseId));
            }

            if (participantId == Guid.Empty)
            {
                throw new ArgumentException("ParticipantId must be a valid GUID.", nameof(participantId));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            }

            Id = Guid.NewGuid();
            ExpenseId = expenseId;
            ParticipantId = participantId;
            Amount = amount;
        }
    }
}
