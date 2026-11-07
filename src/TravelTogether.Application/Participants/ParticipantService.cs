using FluentValidation;
using TravelTogether.Application.Common;
using TravelTogether.Application.Common.Abstractions;

namespace TravelTogether.Application.Participants;

public sealed class ParticipantService(
    ITripRepository trips,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IValidator<ParticipantRequest> validator)
{
    public async Task<IReadOnlyList<ParticipantDto>> ListAsync(Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        return trip.Participants.Select(ParticipantDto.From).ToArray();
    }

    public async Task<ParticipantDto> AddAsync(
        Guid tripId,
        ParticipantRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        var participant = trip.AddParticipant(request.Name, request.Email);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ParticipantDto.From(participant);
    }

    public async Task<ParticipantDto> UpdateAsync(
        Guid tripId,
        Guid participantId,
        ParticipantRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        trip.FindParticipant(participantId);

        var participant = trip.UpdateParticipant(participantId, request.Name, request.Email);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ParticipantDto.From(participant);
    }

    public async Task DeleteAsync(Guid tripId, Guid participantId, CancellationToken cancellationToken)
    {
        var trip = await trips.GetOwnTripAsync(tripId, currentUser, cancellationToken);
        trip.FindParticipant(participantId);

        trip.RemoveParticipant(participantId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
