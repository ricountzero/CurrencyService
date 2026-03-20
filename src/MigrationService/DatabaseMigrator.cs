using Npgsql;

namespace MigrationService;

public class DatabaseMigrator : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseMigrator> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public DatabaseMigrator(
        IConfiguration configuration,
        ILogger<DatabaseMigrator> logger,
        IHostApplicationLifetime lifetime)
    {
        _configuration = configuration;
        _logger = logger;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting database migration...");

            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            for (int attempt = 1; attempt <= 10; attempt++)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync(cancellationToken);
                    await RunMigrationsAsync(conn, cancellationToken);
                    _logger.LogInformation("Database migration completed successfully.");
                    break;
                }
                catch (Exception ex) when (attempt < 10)
                {
                    _logger.LogWarning("Attempt {Attempt}/10 failed: {Message}. Retrying in 3s...", attempt, ex.Message);
                    await Task.Delay(3000, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed.");
            throw;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task RunMigrationsAsync(NpgsqlConnection conn, CancellationToken ct)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS currency (
                id   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(10)  NOT NULL UNIQUE,
                rate NUMERIC(18,6) NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS "user" (
                id       UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                name     VARCHAR(256) NOT NULL UNIQUE,
                password VARCHAR(512) NOT NULL
            );

            CREATE TABLE IF NOT EXISTS user_favorite_currency (
                user_id     UUID NOT NULL REFERENCES "user"(id) ON DELETE CASCADE,
                currency_id UUID NOT NULL REFERENCES currency(id) ON DELETE CASCADE,
                PRIMARY KEY (user_id, currency_id)
            );
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
