using Microsoft.AspNetCore.Mvc;
using TravelTogether.Application.Settlements;

namespace TravelTogether.WebApi.Controllers;

[ApiController]
[Route("api/trips/{tripId:guid}")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
public sealed class SettlementsController(SettlementService settlementService) : ControllerBase
{
    /// <summary>How much each participant paid, their share and net balance.</summary>
    [HttpGet("balances")]
    public Task<BalancesDto> Balances(Guid tripId, CancellationToken cancellationToken) =>
        settlementService.GetBalancesAsync(tripId, cancellationToken);

    /// <summary>The list of transfers ("who pays whom") that settles all debts of the trip.</summary>
    [HttpGet("settlement")]
    public Task<SettlementDto> Settlement(Guid tripId, CancellationToken cancellationToken) =>
        settlementService.GetSettlementAsync(tripId, cancellationToken);
}
