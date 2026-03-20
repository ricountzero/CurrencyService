using FinanceService.Application.Interfaces;
using MediatR;

namespace FinanceService.Application.Commands;

public record RemoveFavoriteCurrencyCommand(Guid UserId, Guid CurrencyId) : IRequest<Unit>;

public class RemoveFavoriteCurrencyCommandHandler : IRequestHandler<RemoveFavoriteCurrencyCommand, Unit>
{
    private readonly IUserFavoritesService _userFavoritesService;

    public RemoveFavoriteCurrencyCommandHandler(IUserFavoritesService userFavoritesService)
    {
        _userFavoritesService = userFavoritesService;
    }

    public async Task<Unit> Handle(RemoveFavoriteCurrencyCommand request, CancellationToken cancellationToken)
    {
        await _userFavoritesService.RemoveFavoriteCurrencyAsync(request.UserId, request.CurrencyId, cancellationToken);
        return Unit.Value;
    }
}
