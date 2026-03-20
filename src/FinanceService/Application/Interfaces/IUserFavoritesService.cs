namespace FinanceService.Application.Interfaces;

public interface IUserFavoritesService
{
    Task<IReadOnlyList<Guid>> GetFavoriteCurrencyIdsAsync(Guid userId, CancellationToken ct = default);
    Task AddFavoriteCurrencyAsync(Guid userId, Guid currencyId, CancellationToken ct = default);
    Task RemoveFavoriteCurrencyAsync(Guid userId, Guid currencyId, CancellationToken ct = default);
}
