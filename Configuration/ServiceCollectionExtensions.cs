using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerceSystem.Database;
using ECommerceSystem.Repositories;
using ECommerceSystem.Services;
using ECommerceSystem.Patterns;
using ECommerceSystem.Utils;

namespace ECommerceSystem.Configuration;

/// <summary>
/// Dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddECommerceServices(this IServiceCollection services, string connectionString)
    {
        // Database
        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<UserRepository>();
        services.AddScoped<ProductRepository>();
        services.AddScoped<OrderRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Services
        services.AddScoped<AuthenticationService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();

        // Patterns
        services.AddSingleton(typeof(ICacheDecorator<>), typeof(InMemoryCacheDecorator<>));
        services.AddScoped<PaymentProcessor>();
        
        // Utilities
        services.AddScoped<ExceptionHandler>();

        // Logging
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddConsole();
        });

        return services;
    }

    public static IServiceProvider AddPaymentStrategies(this IServiceProvider serviceProvider)
    {
        // Register payment strategies here if needed
        return serviceProvider;
    }
}
