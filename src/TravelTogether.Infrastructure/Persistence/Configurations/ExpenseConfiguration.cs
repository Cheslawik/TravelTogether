using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence.Configurations;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).HasMaxLength(Expense.TitleMaxLength).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.SplitType).HasConversion<string>().HasMaxLength(20);

        // NO ACTION (not RESTRICT) so deleting a whole trip can remove participants
        // and expenses in one statement; removing a used participant alone still fails.
        builder.HasOne<Participant>()
            .WithMany()
            .HasForeignKey(e => e.PaidByParticipantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(e => e.Splits)
            .WithOne()
            .HasForeignKey(s => s.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Splits).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
