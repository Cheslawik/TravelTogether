using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseSplitConfiguration : IEntityTypeConfiguration<ExpenseSplit>
{
    public void Configure(EntityTypeBuilder<ExpenseSplit> builder)
    {
        builder.ToTable("ExpenseSplits");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Amount).HasPrecision(18, 2);

        builder.HasOne<Participant>()
            .WithMany()
            .HasForeignKey(s => s.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => new { s.ExpenseId, s.ParticipantId }).IsUnique();
    }
}
