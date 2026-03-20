using FinanceService.Application.Interfaces;
using FinanceService.Domain.Repositories;
using FinanceService.Infrastructure.Persistence;
using FinanceService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddSingleton(new DbConnectionFactory(connectionString));
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IUserFavoritesService, UserFavoritesService>();
        return services;
    }
}
