using MigrationService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<DatabaseMigrator>();
    })
    .Build();

await host.RunAsync();
