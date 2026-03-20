using FinanceService.Domain.Entities;
using FinanceService.Domain.Repositories;
using FinanceService.Infrastructure.Persistence;
using Npgsql;

namespace FinanceService.Infrastructure.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly DbConnectionFactory _factory;

    public CurrencyRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Currency>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToArray();
        if (idList.Length == 0) return Array.Empty<Currency>();

        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, rate FROM currency WHERE id = ANY(@ids)", conn);
        cmd.Parameters.AddWithValue("ids", idList);

        return await ReadCurrenciesAsync(cmd, ct);
    }

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, rate FROM currency ORDER BY name", conn);

        return await ReadCurrenciesAsync(cmd, ct);
    }

    private static async Task<IReadOnlyList<Currency>> ReadCurrenciesAsync(NpgsqlCommand cmd, CancellationToken ct)
    {
        var result = new List<Currency>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new Currency
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Rate = reader.GetDecimal(2)
            });
        }
        return result;
    }
}
