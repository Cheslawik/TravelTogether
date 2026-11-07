using Microsoft.AspNetCore.Mvc;
using TravelTogether.Application.Participants;

namespace TravelTogether.WebApi.Controllers;

[ApiController]
[Route("api/trips/{tripId:guid}/participants")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
public sealed class ParticipantsController(ParticipantService participantService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ParticipantDto>> List(Guid tripId, CancellationToken cancellationToken) =>
        participantService.ListAsync(tripId, cancellationToken);

    /// <summary>Adds a participant. Names (and emails, if given) must be unique within the trip.</summary>
    [HttpPost]
    [ProducesResponseType<ParticipantDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ParticipantDto>> Add(
        Guid tripId,
        ParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var participant = await participantService.AddAsync(tripId, request, cancellationToken);
        return CreatedAtAction(nameof(List), new { tripId }, participant);
    }

    [HttpPut("{participantId:guid}")]
    [ProducesResponseType<ParticipantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ParticipantDto>> Update(
        Guid tripId,
        Guid participantId,
        ParticipantRequest request,
        CancellationToken cancellationToken) =>
        await participantService.UpdateAsync(tripId, participantId, request, cancellationToken);

    /// <summary>Removes a participant who is not used in any expense.</summary>
    [HttpDelete("{participantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid tripId, Guid participantId, CancellationToken cancellationToken)
    {
        await participantService.DeleteAsync(tripId, participantId, cancellationToken);
        return NoContent();
    }
}
