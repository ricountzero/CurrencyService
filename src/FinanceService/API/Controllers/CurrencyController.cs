using System.Security.Claims;
using FinanceService.Application.Commands;
using FinanceService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CurrencyController : ControllerBase
{
    private readonly IMediator _mediator;

    public CurrencyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all currency rates (no filter).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllRatesQuery(), ct);
        return Ok(result.Rates);
    }

    /// <summary>
    /// Get currency rates for the authenticated user's favorites.
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRates(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetRatesForUserQuery(userId), ct);
        return Ok(result.Rates);
    }

    /// <summary>
    /// Add a currency to the authenticated user's favorites.
    /// </summary>
    [HttpPost("favorites/{currencyId:guid}")]
    public async Task<IActionResult> AddFavorite(Guid currencyId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new AddFavoriteCurrencyCommand(userId, currencyId), ct);
        return NoContent();
    }

    /// <summary>
    /// Remove a currency from the authenticated user's favorites.
    /// </summary>
    [HttpDelete("favorites/{currencyId:guid}")]
    public async Task<IActionResult> RemoveFavorite(Guid currencyId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _mediator.Send(new RemoveFavoriteCurrencyCommand(userId, currencyId), ct);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim missing.");
        return Guid.Parse(claim);
    }
}
