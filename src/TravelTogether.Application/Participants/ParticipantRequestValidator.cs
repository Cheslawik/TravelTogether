using FluentValidation;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Participants;

public sealed class ParticipantRequestValidator : AbstractValidator<ParticipantRequest>
{
    public ParticipantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Participant.NameMaxLength);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(Participant.EmailMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
