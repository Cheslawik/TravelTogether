using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Infrastructure.Persistence.Configurations;

internal sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).HasMaxLength(Participant.NameMaxLength).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(Participant.EmailMaxLength);
    }
}
