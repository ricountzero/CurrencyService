using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Npgsql;

namespace CurrencyBackgroundService;

public class CbrCurrencyWorker : BackgroundService
{
    private const string CbrUrl = "http://www.cbr.ru/scripts/XML_daily.asp";
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CbrCurrencyWorker> _logger;

    public CbrCurrencyWorker(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<CbrCurrencyWorker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        // Register windows-1251 encoding support
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await FetchAndSaveRatesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CBR currency rates.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task FetchAndSaveRatesAsync(CancellationToken ct)
    {
        _logger.LogInformation("Fetching currency rates from CBR...");

        var client = _httpClientFactory.CreateClient();

        // Read as bytes and decode as windows-1251 manually
        var bytes = await client.GetByteArrayAsync(CbrUrl, ct);
        var encoding = Encoding.GetEncoding("windows-1251");
        var xml = encoding.GetString(bytes);

        var rates = ParseCbrXml(xml);
        _logger.LogInformation("Parsed {Count} currency rates.", rates.Count);

        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        foreach (var (name, rate) in rates)
        {
            var sql = """
                INSERT INTO currency (id, name, rate)
                VALUES (gen_random_uuid(), @name, @rate)
                ON CONFLICT (name) DO UPDATE SET rate = EXCLUDED.rate;
                """;
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("rate", rate);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        _logger.LogInformation("Currency rates saved to database.");
    }

    private static List<(string Name, decimal Rate)> ParseCbrXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        var result = new List<(string, decimal)>();

        foreach (var valute in doc.Root?.Elements("Valute") ?? Enumerable.Empty<XElement>())
        {
            var charCode = valute.Element("CharCode")?.Value;
            var nominalStr = valute.Element("Nominal")?.Value ?? "1";
            var valueStr = valute.Element("Value")?.Value ?? "0";

            if (string.IsNullOrWhiteSpace(charCode)) continue;

            var nominal = decimal.Parse(nominalStr, CultureInfo.InvariantCulture);
            var value = decimal.Parse(valueStr.Replace(',', '.'), CultureInfo.InvariantCulture);

            var ratePerUnit = nominal > 0 ? value / nominal : 0;
            result.Add((charCode, ratePerUnit));
        }

        return result;
    }
}
