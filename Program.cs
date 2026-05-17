using Microsoft.Extensions.DependencyInjection;
using ECommerceSystem.Configuration;
using ECommerceSystem.Services;
using ECommerceSystem.DTOs;
using ECommerceSystem.Models;
using ECommerceSystem.Utils;
using Serilog;

// Configure logging
LoggingConfiguration.ConfigureLogging();

try
{
    // Connection string (update with your SQL Server connection)
    string connectionString = "Server=.;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;";

    // Setup dependency injection
    var services = new ServiceCollection();
    services.AddECommerceServices(connectionString);
    var serviceProvider = services.BuildServiceProvider();

    // Get services
    var authService = serviceProvider.GetRequiredService<AuthenticationService>();
    var productService = serviceProvider.GetRequiredService<ProductService>();
    var orderService = serviceProvider.GetRequiredService<OrderService>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("==== E-Commerce System Started ====");

    // Example: Register a customer
    logger.LogInformation("\n--- Registering Customer ---");
    var customer = await authService.RegisterAsync(
        "john_doe",
        "john@example.com",
        "password123",
        "John",
        "Doe",
        UserRole.Customer
    );
    logger.LogInformation($"Customer registered: {customer.GetFullName()} (ID: {customer.Id})");

    // Example: Register admin
    logger.LogInformation("\n--- Registering Admin ---");
    var admin = await authService.RegisterAsync(
        "admin_user",
        "admin@example.com",
        "admin123",
        "Admin",
        "User",
        UserRole.Admin
    );
    logger.LogInformation($"Admin registered: {admin.GetFullName()} (ID: {admin.Id})");

    // Example: Login
    logger.LogInformation("\n--- Customer Login ---");
    var loggedInUser = await authService.LoginAsync("john_doe", "password123");
    logger.LogInformation($"Login successful: {loggedInUser.GetFullName()}");

    logger.LogInformation("\n==== E-Commerce System Demo Complete ====");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
