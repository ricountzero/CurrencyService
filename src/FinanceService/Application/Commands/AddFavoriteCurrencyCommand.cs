using FinanceService.Application.Interfaces;
using MediatR;

namespace FinanceService.Application.Commands;

public record AddFavoriteCurrencyCommand(Guid UserId, Guid CurrencyId) : IRequest<Unit>;

public class AddFavoriteCurrencyCommandHandler : IRequestHandler<AddFavoriteCurrencyCommand, Unit>
{
    private readonly IUserFavoritesService _userFavoritesService;

    public AddFavoriteCurrencyCommandHandler(IUserFavoritesService userFavoritesService)
    {
        _userFavoritesService = userFavoritesService;
    }

    public async Task<Unit> Handle(AddFavoriteCurrencyCommand request, CancellationToken cancellationToken)
    {
        await _userFavoritesService.AddFavoriteCurrencyAsync(request.UserId, request.CurrencyId, cancellationToken);
        return Unit.Value;
    }
}
