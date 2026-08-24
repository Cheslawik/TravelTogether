namespace WebApplication1.Domain.Entities
{
    public class Trip
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = null!;
        public string? Description { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private readonly List<Participant> _participants = [];
        private readonly List<Expense> _expenses = [];

        public IReadOnlyCollection<Participant> Participants => _participants;
        public IReadOnlyCollection<Expense> Expenses => _expenses;

        private Trip() { }

        public Trip(
            string title,
            string? description,
            DateTime startDate,
            DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(
                    "Title cannot be empty.",
                    nameof(title));

            if (startDate == default)
                throw new ArgumentException(
                    "Start date is required.",
                    nameof(startDate));

            if (endDate == default)
                throw new ArgumentException(
                    "End date is required.",
                    nameof(endDate));

            if (startDate > endDate)
                throw new InvalidOperationException(
                    "Start date cannot be after end date.");

            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(
            string title,
            string? description,
            DateTime startDate,
            DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(
                    "Title cannot be empty.",
                    nameof(title));

            if (startDate == default)
                throw new ArgumentException(
                    "Start date is required.",
                    nameof(startDate));

            if (endDate == default)
                throw new ArgumentException(
                    "End date is required.",
                    nameof(endDate));

            if (startDate > endDate)
                throw new InvalidOperationException(
                    "Start date cannot be after end date.");

            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
        }

        public Participant AddParticipant(
            string name,
            string email)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Participant name is required.",
                    nameof(name));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "Participant email is required.",
                    nameof(email));
            }

            var participantAlreadyExists = Participants.Any(
                p => string.Equals(email, p.Email, StringComparison.OrdinalIgnoreCase));

            if (participantAlreadyExists)
            {
                throw new InvalidOperationException(
                    "A participant already exists in this trip.");
            }

            var participant = new Participant(Id, name, email);

            _participants.Add(participant);
            return participant;
        }

        public Expense AddExpense(
            Guid paidByParticipantId, 
            string title, 
            decimal amount, 
            DateTime date, 
            IReadOnlyCollection<Guid> splitParticipantIds)
        {
            ArgumentNullException.ThrowIfNull(splitParticipantIds);

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Expense title is required.",
                    nameof(title));
            }

            if (amount <= 0)
            {
                throw new ArgumentException(
                    "Expense amount must be greater than zero.",
                    nameof(amount));
            }

            var participantExists = Participants.Any(p => p.Id == paidByParticipantId);

            if (!participantExists)
            {
                throw new InvalidOperationException(
                    "The participant paying for the expense must be part of the trip.");
            }

            if (splitParticipantIds.Count == 0)
            { 
                throw new ArgumentException(
                    "At least one participant must be included in the expense split.",
                    nameof(splitParticipantIds));
            }

            var uniqueParticipantIds = splitParticipantIds
                .Distinct()
                .ToArray();

            if (uniqueParticipantIds.Length != splitParticipantIds.Count)
            {
                throw new ArgumentException(
                    "A participant cannot be included in an expense split more than once.",
                    nameof(splitParticipantIds));
            }

            var hasUnknownParticipants = uniqueParticipantIds
                .Any(id => !_participants.Any(p => p.Id == id));

            if (hasUnknownParticipants)
            {
                throw new InvalidOperationException(
                    "All split participants must be part of the trip.");
            }

            var expense = new Expense(
                Id,
                paidByParticipantId,
                title,
                amount,
                date);

            expense.SplitEqually(uniqueParticipantIds);

            _expenses.Add(expense);

            return expense;
        }
    }
}