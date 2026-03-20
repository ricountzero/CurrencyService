using Npgsql;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            @"SELECT id, name, password FROM ""user"" WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var user = MapUser(reader);
        await reader.CloseAsync();
        await LoadFavoritesAsync(conn, user, ct);
        return user;
    }

    public async Task<User?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            @"SELECT id, name, password FROM ""user"" WHERE name = @name", conn);
        cmd.Parameters.AddWithValue("name", name);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var user = MapUser(reader);
        await reader.CloseAsync();
        await LoadFavoritesAsync(conn, user, ct);
        return user;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO ""user"" (id, name, password) VALUES (@id, @name, @password)", conn);
        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.Parameters.AddWithValue("name", user.Name);
        cmd.Parameters.AddWithValue("password", user.Password);
        await cmd.ExecuteNonQueryAsync(ct);

        await SaveFavoritesAsync(conn, user, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await using var conn = _factory.CreateConnection();
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            @"UPDATE ""user"" SET name = @name, password = @password WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.Parameters.AddWithValue("name", user.Name);
        cmd.Parameters.AddWithValue("password", user.Password);
        await cmd.ExecuteNonQueryAsync(ct);

        await using var delCmd = new NpgsqlCommand(
            "DELETE FROM user_favorite_currency WHERE user_id = @uid", conn);
        delCmd.Parameters.AddWithValue("uid", user.Id);
        await delCmd.ExecuteNonQueryAsync(ct);

        await SaveFavoritesAsync(conn, user, ct);
    }

    private static User MapUser(NpgsqlDataReader r) =>
        User.Reconstitute(r.GetGuid(0), r.GetString(1), r.GetString(2));

    private static async Task LoadFavoritesAsync(NpgsqlConnection conn, User user, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT currency_id FROM user_favorite_currency WHERE user_id = @uid", conn);
        cmd.Parameters.AddWithValue("uid", user.Id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            user.AddFavorite(reader.GetGuid(0));
    }

    private static async Task SaveFavoritesAsync(NpgsqlConnection conn, User user, CancellationToken ct)
    {
        foreach (var currencyId in user.FavoriteCurrencyIds)
        {
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO user_favorite_currency (user_id, currency_id) VALUES (@uid, @cid) ON CONFLICT DO NOTHING",
                conn);
            cmd.Parameters.AddWithValue("uid", user.Id);
            cmd.Parameters.AddWithValue("cid", currencyId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
