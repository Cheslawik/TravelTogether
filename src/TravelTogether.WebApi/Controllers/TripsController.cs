using Microsoft.AspNetCore.Mvc;
using TravelTogether.Application.Trips;

namespace TravelTogether.WebApi.Controllers;

[ApiController]
[Route("api/trips")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class TripsController(TripService tripService) : ControllerBase
{
    /// <summary>Lists trips of the current user, newest first.</summary>
    [HttpGet]
    public Task<IReadOnlyList<TripSummaryDto>> List(CancellationToken cancellationToken) =>
        tripService.ListAsync(cancellationToken);

    /// <summary>Returns a trip with its participants and total spending.</summary>
    [HttpGet("{tripId:guid}")]
    [ProducesResponseType<TripDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripDto>> Get(Guid tripId, CancellationToken cancellationToken) =>
        await tripService.GetAsync(tripId, cancellationToken);

    [HttpPost]
    [ProducesResponseType<TripDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TripDto>> Create(TripRequest request, CancellationToken cancellationToken)
    {
        var trip = await tripService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { tripId = trip.Id }, trip);
    }

    /// <summary>Updates trip details. The currency can only change while the trip has no expenses.</summary>
    [HttpPut("{tripId:guid}")]
    [ProducesResponseType<TripDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TripDto>> Update(Guid tripId, TripRequest request, CancellationToken cancellationToken) =>
        await tripService.UpdateAsync(tripId, request, cancellationToken);

    /// <summary>Deletes a trip with all its participants and expenses.</summary>
    [HttpDelete("{tripId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid tripId, CancellationToken cancellationToken)
    {
        await tripService.DeleteAsync(tripId, cancellationToken);
        return NoContent();
    }
}
