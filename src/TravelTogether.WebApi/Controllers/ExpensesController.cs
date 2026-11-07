using Microsoft.AspNetCore.Mvc;
using TravelTogether.Application.Expenses;

namespace TravelTogether.WebApi.Controllers;

[ApiController]
[Route("api/trips/{tripId:guid}/expenses")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
public sealed class ExpensesController(ExpenseService expenseService) : ControllerBase
{
    /// <summary>Lists expenses of a trip ordered by date.</summary>
    [HttpGet]
    public Task<IReadOnlyList<ExpenseDto>> List(Guid tripId, CancellationToken cancellationToken) =>
        expenseService.ListAsync(tripId, cancellationToken);

    [HttpGet("{expenseId:guid}")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpenseDto>> Get(Guid tripId, Guid expenseId, CancellationToken cancellationToken) =>
        await expenseService.GetAsync(tripId, expenseId, cancellationToken);

    /// <summary>Adds an expense split equally or by shares between participants.</summary>
    [HttpPost]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ExpenseDto>> Add(Guid tripId, ExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = await expenseService.AddAsync(tripId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { tripId, expenseId = expense.Id }, expense);
    }

    [HttpPut("{expenseId:guid}")]
    [ProducesResponseType<ExpenseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ExpenseDto>> Update(
        Guid tripId,
        Guid expenseId,
        ExpenseRequest request,
        CancellationToken cancellationToken) =>
        await expenseService.UpdateAsync(tripId, expenseId, request, cancellationToken);

    [HttpDelete("{expenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid tripId, Guid expenseId, CancellationToken cancellationToken)
    {
        await expenseService.DeleteAsync(tripId, expenseId, cancellationToken);
        return NoContent();
    }
}
