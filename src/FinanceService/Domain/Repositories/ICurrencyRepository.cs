using FinanceService.Domain.Entities;

namespace FinanceService.Domain.Repositories;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default);
}
