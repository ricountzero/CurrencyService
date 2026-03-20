using FinanceService.Domain.Repositories;
using MediatR;

namespace FinanceService.Application.Queries;

public record GetAllRatesQuery : IRequest<GetRatesForUserResult>;

public class GetAllRatesQueryHandler : IRequestHandler<GetAllRatesQuery, GetRatesForUserResult>
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetAllRatesQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<GetRatesForUserResult> Handle(GetAllRatesQuery request, CancellationToken cancellationToken)
    {
        var currencies = await _currencyRepository.GetAllAsync(cancellationToken);
        var rates = currencies.Select(c => new CurrencyRateDto(c.Id, c.Name, c.Rate)).ToList();
        return new GetRatesForUserResult(rates);
    }
}
