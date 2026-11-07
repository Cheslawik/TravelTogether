using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title).HasMaxLength(Trip.TitleMaxLength).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(Trip.DescriptionMaxLength);
        builder.Property(t => t.Currency).HasMaxLength(Trip.CurrencyLength).IsFixedLength().IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(t => t.OwnerId);

        builder.HasMany(t => t.Participants)
            .WithOne()
            .HasForeignKey(p => p.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Expenses)
            .WithOne()
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        // The aggregate exposes read-only collections; EF writes to the private lists.
        builder.Navigation(t => t.Participants).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Expenses).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
