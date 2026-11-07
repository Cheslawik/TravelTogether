using FluentValidation;
using TravelTogether.Domain.Entities;

namespace TravelTogether.Application.Trips;

public sealed class TripRequestValidator : AbstractValidator<TripRequest>
{
    public TripRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Trip.TitleMaxLength);
        RuleFor(x => x.Description).MaximumLength(Trip.DescriptionMaxLength);
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must be a three-letter ISO 4217 code, e.g. EUR.");
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date cannot be before start date.");
    }
}
