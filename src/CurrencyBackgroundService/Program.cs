using CurrencyBackgroundService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHttpClient();
        services.AddHostedService<CbrCurrencyWorker>();
    })
    .Build();

await host.RunAsync();
