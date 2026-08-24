namespace WebApplication1.Domain.Entities
{
    public class Participant
    {
        public Guid Id { get; private set; }
        public Guid TripId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        
        public Trip Trip { get; private set; } = null!;
        public ICollection<Expense> PaidExpenses { get; private set; } = new List<Expense>();
        public ICollection<ExpenseSplit> ExpenseSplits { get; private set; } = new List<ExpenseSplit>();

        
        private Participant() { }

        internal Participant(Guid tripId, string name, string email)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            }
            
            if(string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            }
            
            if(tripId == Guid.Empty)
            {
                throw new ArgumentException(
                    "TripId must be a valid GUID.",
                    nameof(tripId));
            }

            Id = Guid.NewGuid();
            TripId = tripId;
            Name = name;
            Email = email;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
