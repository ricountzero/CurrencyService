using FinanceService.Application.Interfaces;
using FinanceService.Infrastructure.Persistence;
using Npgsql;

namespace FinanceService.Infrastructure.Repositories;

public class UserFavoritesService : IUserFavoritesService
{
    private readonly DbConnectionFactory _factory;

    public UserFavoritesService(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Guid>> GetFavoriteCurrencyIdsAsync(Guid userId, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT currency_id FROM user_favorite_currency WHERE user_id = @uid", conn);
        cmd.Parameters.AddWithValue("uid", userId);

        var ids = new List<Guid>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            ids.Add(reader.GetGuid(0));

        return ids;
    }

    public async Task AddFavoriteCurrencyAsync(Guid userId, Guid currencyId, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "INSERT INTO user_favorite_currency (user_id, currency_id) VALUES (@uid, @cid) ON CONFLICT DO NOTHING", conn);
        cmd.Parameters.AddWithValue("uid", userId);
        cmd.Parameters.AddWithValue("cid", currencyId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RemoveFavoriteCurrencyAsync(Guid userId, Guid currencyId, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "DELETE FROM user_favorite_currency WHERE user_id = @uid AND currency_id = @cid", conn);
        cmd.Parameters.AddWithValue("uid", userId);
        cmd.Parameters.AddWithValue("cid", currencyId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
