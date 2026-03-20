using FinanceService.Application.Interfaces;
using FinanceService.Domain.Repositories;
using MediatR;

namespace FinanceService.Application.Queries;

public record GetRatesForUserQuery(Guid UserId) : IRequest<GetRatesForUserResult>;

public record CurrencyRateDto(Guid Id, string Name, decimal Rate);

public record GetRatesForUserResult(IReadOnlyList<CurrencyRateDto> Rates);

public class GetRatesForUserQueryHandler : IRequestHandler<GetRatesForUserQuery, GetRatesForUserResult>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserFavoritesService _userFavoritesService;

    public GetRatesForUserQueryHandler(
        ICurrencyRepository currencyRepository,
        IUserFavoritesService userFavoritesService)
    {
        _currencyRepository = currencyRepository;
        _userFavoritesService = userFavoritesService;
    }

    public async Task<GetRatesForUserResult> Handle(GetRatesForUserQuery request, CancellationToken cancellationToken)
    {
        var favoriteIds = await _userFavoritesService.GetFavoriteCurrencyIdsAsync(request.UserId, cancellationToken);

        if (favoriteIds.Count == 0)
            return new GetRatesForUserResult(Array.Empty<CurrencyRateDto>());

        var currencies = await _currencyRepository.GetByIdsAsync(favoriteIds, cancellationToken);

        var rates = currencies
            .Select(c => new CurrencyRateDto(c.Id, c.Name, c.Rate))
            .ToList();

        return new GetRatesForUserResult(rates);
    }
}
