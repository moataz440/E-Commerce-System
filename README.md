# E-Commerce System - Professional C# Implementation

## 🎯 Project Overview

A comprehensive, production-ready e-commerce system built with **C# .NET 8**, demonstrating advanced OOP principles and design patterns. This project is designed for enterprise-level applications and serves as an excellent portfolio piece.

## ✨ Key Features

### Architecture & Design Patterns
- **Repository Pattern** - Clean data access abstraction
- **Strategy Pattern** - Flexible payment processing
- **Decorator Pattern** - Intelligent caching system
- **Dependency Injection** - Loose coupling and testability
- **Factory Pattern** - Payment processor creation
- **Singleton** - Logging configuration

### OOP Concepts
- ✅ **Inheritance** - User hierarchy, Entity base classes
- ✅ **Polymorphism** - Payment strategies, Repository generics
- ✅ **Encapsulation** - Property protection, business logic isolation
- ✅ **Abstraction** - Interfaces for repositories and payment methods

### Core Functionality
- 👤 User Management with Role-Based Access Control (Admin, Customer, Vendor)
- 🔐 Secure Authentication with BCrypt Password Hashing
- 📦 Product Management with Stock Control
- 🛒 Order Processing with Status Tracking
- ⭐ Product Reviews and Ratings
- 💳 Multiple Payment Methods (Strategy Pattern)
- 📊 Advanced Reporting and Analytics
- 🔍 Search and Filter Capabilities
- 📝 Comprehensive Logging with Serilog

### Database
- SQL Server with Entity Framework Core
- Fully normalized schema with relationships
- Indexes for performance optimization
- Stored procedures and views for analytics
- Seed data for testing

## 📁 Project Structure

```
ECommerceSystem/
├── Models/                    # Domain models
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── Review.cs
│   └── Category.cs
├── Database/                  # Data layer
│   ├── ECommerceDbContext.cs
│   └── Setup.sql
├── Repositories/              # Data access
│   ├── Repository.cs
│   ├── UserRepository.cs
│   ├── ProductRepository.cs
│   └── OrderRepository.cs
├── Services/                  # Business logic
│   ├── AuthenticationService.cs
│   ├── ProductService.cs
│   └── OrderService.cs
├── Interfaces/                # Contracts
│   └── IRepository.cs
├── DTOs/                      # Data transfer objects
│   └── DTOs.cs
├── Patterns/                  # Design patterns
│   ├── PaymentStrategies.cs
│   └── CacheDecorator.cs
├── Exceptions/                # Custom exceptions
│   └── ECommerceExceptions.cs
├── Utils/                     # Utilities
│   ├── LoggingConfiguration.cs
│   └── ExceptionHandler.cs
├── Configuration/             # DI setup
│   └── ServiceCollectionExtensions.cs
├── Program.cs                 # Entry point
└── ECommerceSystem.csproj     # Project file
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

### Installation

1. **Clone/Extract the project**
```bash
cd ECommerceSystem
```

2. **Update Connection String** (in Program.cs)
```csharp
string connectionString = "Server=YOUR_SERVER;Database=ECommerceDB;Trusted_Connection=True;";
```

3. **Install Dependencies**
```bash
dotnet restore
```

4. **Create Database**
```bash
# Option 1: Run SQL script
sqlcmd -S YOUR_SERVER -i Database/Setup.sql

# Option 2: Use Entity Framework migrations
dotnet ef database update
```

5. **Run Application**
```bash
dotnet run
```

## 💡 Design Patterns Implemented

### 1. **Repository Pattern**
```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

### 2. **Strategy Pattern**
```csharp
public interface IPaymentStrategy
{
    Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails);
}

// Multiple implementations: CreditCardPaymentStrategy, PayPalPaymentStrategy, etc.
```

### 3. **Decorator Pattern**
```csharp
public class InMemoryCacheDecorator<T> : ICacheDecorator<T>
{
    public async Task<T?> GetOrExecuteAsync(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        // Cache logic with expiration
    }
}
```

### 4. **Dependency Injection**
```csharp
services.AddECommerceServices(connectionString);
```

## 📚 Usage Examples

### Authentication
```csharp
var customer = await authService.RegisterAsync(
    "john_doe",
    "john@example.com",
    "password123",
    "John",
    "Doe",
    UserRole.Customer
);

var user = await authService.LoginAsync("john_doe", "password123");
```

### Product Management
```csharp
var product = await productService.CreateProductAsync(new CreateProductDto
{
    Name = "Laptop",
    Description = "High-performance laptop",
    Price = 999.99m,
    StockQuantity = 50,
    SKU = "LAPTOP-001",
    CategoryId = 1
});

var products = await productService.SearchProductsAsync("laptop");
var lowStock = await productService.GetLowStockProductsAsync(10);
```

### Order Processing
```csharp
var order = await orderService.CreateOrderAsync(new CreateOrderDto
{
    UserId = 1,
    Items = new List<OrderItemDto>
    {
        new OrderItemDto { ProductId = 1, Quantity = 2 }
    },
    ShippingAddress = "123 Main St"
});

await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Shipped);
```

### Payment Processing (Strategy Pattern)
```csharp
var paymentProcessor = serviceProvider.GetRequiredService<PaymentProcessor>();

// Use Credit Card
paymentProcessor.SetPaymentStrategy(new CreditCardPaymentStrategy(logger));
await paymentProcessor.PayAsync(99.99m, "card_details");

// Switch to PayPal
paymentProcessor.SetPaymentStrategy(new PayPalPaymentStrategy(logger));
await paymentProcessor.PayAsync(99.99m, "paypal_email");
```

## 🔒 Security Features

- **Password Hashing** - BCrypt encryption
- **Role-Based Access Control** - User role validation
- **Exception Handling** - Custom exception hierarchy
- **Input Validation** - DTO validation
- **SQL Injection Prevention** - Entity Framework parameterized queries

## 📊 Database Schema

### Entity Relationships
```
User (1) ---> (Many) Order
User (1) ---> (Many) Review
Product (1) ---> (Many) OrderItem
Product (1) ---> (Many) Review
Category (1) ---> (Many) Product
Order (1) ---> (Many) OrderItem
```

## 🧪 Testing Queries

```csharp
// Get total revenue
var revenue = await orderService.GetTotalRevenueAsync();

// Get low stock products
var lowStock = await productService.GetLowStockProductsAsync(10);

// Get user orders
var userOrders = await orderService.GetUserOrdersAsync(userId);

// Get inventory value
var inventoryValue = await productService.GetInventoryValueAsync();
```

## 📈 Performance Optimization

- ✅ Entity Framework query optimization with Include/ThenInclude
- ✅ In-memory caching decorator for frequently accessed data
- ✅ Database indexes on frequently queried columns
- ✅ Async/await throughout for non-blocking operations
- ✅ Connection pooling via EF Core

## 🛠️ Technologies Used

- **Framework**: .NET 8
- **Database**: SQL Server with Entity Framework Core
- **Logging**: Serilog
- **Security**: BCrypt for password hashing
- **Architecture**: Clean Architecture, SOLID Principles
- **Patterns**: Repository, Strategy, Decorator, Factory, Singleton

## 📝 Logging

Logs are generated to both console and file:
```
logs/ecommerce-YYYYMMDD.txt
```

## 🎓 Learning Outcomes

This project demonstrates:
- Advanced OOP principles
- Enterprise design patterns
- Database design and optimization
- Dependency injection and IoC containers
- Exception handling best practices
- Async programming patterns
- SOLID principles application
- Clean code practices

## 📄 License

This project is provided as-is for educational and portfolio purposes.

## 🤝 Contributing

Feel free to fork and improve!

---

**Built with ❤️ for Excellence in Software Engineering**
