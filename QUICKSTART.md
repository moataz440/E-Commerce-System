# E-Commerce System - Quick Start Guide

## 🚀 First Steps

### 1. Database Setup
```sql
-- Run this in SQL Server Management Studio
-- File: Database/Setup.sql
sqlcmd -S YOUR_SERVER -i Database/Setup.sql
```

### 2. Update Connection String
**File: Program.cs (Line ~24)**
```csharp
string connectionString = "Server=YOUR_SERVER;Database=ECommerceDB;Trusted_Connection=True;TrustServerCertificate=True;";
```

### 3. Install & Run
```bash
dotnet restore
dotnet run
```

## 🏗️ Project Architecture

### Layer 1: Models (Domain)
- Represents business entities
- Contains validation logic
- Example: `User.cs`, `Product.cs`, `Order.cs`

### Layer 2: Database (Data Access)
- Entity Framework Core DbContext
- Manages database communication
- File: `Database/ECommerceDbContext.cs`

### Layer 3: Repositories (Data Abstraction)
- Implements Repository Pattern
- Provides generic CRUD operations
- Examples: `UserRepository.cs`, `ProductRepository.cs`

### Layer 4: Services (Business Logic)
- Contains business rules
- Coordinates between repositories
- Examples: `AuthenticationService.cs`, `OrderService.cs`

### Layer 5: DTOs (API Contracts)
- Data Transfer Objects
- Communication between layers
- File: `DTOs/DTOs.cs`

## 💡 Key Design Patterns

### 1. Repository Pattern
**Purpose**: Abstract database operations
```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

**Where to see it**: `Repositories/` folder

### 2. Strategy Pattern
**Purpose**: Flexible payment processing
```csharp
public interface IPaymentStrategy
{
    Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails);
}
```

**Implementation**: 
- `CreditCardPaymentStrategy`
- `PayPalPaymentStrategy`
- `StripePaymentStrategy`

**File**: `Patterns/PaymentStrategies.cs`

### 3. Decorator Pattern
**Purpose**: Add caching to data access
```csharp
public class InMemoryCacheDecorator<T> : ICacheDecorator<T>
{
    public async Task<T?> GetOrExecuteAsync(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
}
```

**File**: `Patterns/CacheDecorator.cs`

### 4. Dependency Injection
**Purpose**: Loose coupling and testability
```csharp
services.AddECommerceServices(connectionString);
var authService = serviceProvider.GetRequiredService<AuthenticationService>();
```

**File**: `Configuration/ServiceCollectionExtensions.cs`

## 📊 Common Tasks

### Register a Customer
```csharp
var authService = serviceProvider.GetRequiredService<AuthenticationService>();

var customer = await authService.RegisterAsync(
    username: "john_doe",
    email: "john@example.com",
    password: "securePassword123",
    firstName: "John",
    lastName: "Doe",
    role: UserRole.Customer
);

Console.WriteLine($"Customer created: {customer.GetFullName()}");
```

### Login User
```csharp
try
{
    var user = await authService.LoginAsync("john_doe", "securePassword123");
    Console.WriteLine($"Welcome {user.GetFullName()}!");
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Login failed: {ex.Message}");
}
```

### Create Product
```csharp
var productService = serviceProvider.GetRequiredService<ProductService>();

var product = await productService.CreateProductAsync(new CreateProductDto
{
    Name = "Gaming Laptop",
    Description = "High-performance laptop for gaming",
    Price = 1299.99m,
    StockQuantity = 25,
    SKU = "LAPTOP-GAMING-001",
    CategoryId = 1 // Electronics
});

Console.WriteLine($"Product created: {product.Name} - ${product.Price}");
```

### Search Products
```csharp
var results = await productService.SearchProductsAsync("laptop");
foreach (var product in results)
{
    Console.WriteLine($"- {product.Name}: ${product.Price}");
}
```

### Create Order
```csharp
var orderService = serviceProvider.GetRequiredService<OrderService>();

var order = await orderService.CreateOrderAsync(new CreateOrderDto
{
    UserId = 1,
    Items = new List<OrderItemDto>
    {
        new OrderItemDto { ProductId = 1, Quantity = 2 },
        new OrderItemDto { ProductId = 3, Quantity = 1 }
    },
    ShippingAddress = "123 Main Street, New York, NY 10001",
    Notes = "Please deliver on weekdays only"
});

Console.WriteLine($"Order created: {order.OrderNumber} - ${order.TotalAmount:C}");
```

### Update Order Status
```csharp
await orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Shipped);
Console.WriteLine("Order status updated to Shipped");
```

### Process Payment (Strategy Pattern Example)
```csharp
var paymentProcessor = serviceProvider.GetRequiredService<PaymentProcessor>();

// Pay with Credit Card
var creditCardStrategy = new CreditCardPaymentStrategy(logger);
paymentProcessor.SetPaymentStrategy(creditCardStrategy);
bool success = await paymentProcessor.PayAsync(299.99m, "4532-xxxx-xxxx-1111");

// Switch to PayPal
var paypalStrategy = new PayPalPaymentStrategy(logger);
paymentProcessor.SetPaymentStrategy(paypalStrategy);
success = await paymentProcessor.PayAsync(299.99m, "user@paypal.com");
```

### Get Business Metrics
```csharp
// Total Revenue
var totalRevenue = await orderService.GetTotalRevenueAsync();
Console.WriteLine($"Total Revenue: ${totalRevenue:C}");

// Average Order Value
var avgOrderValue = await orderService.GetAverageOrderValueAsync();
Console.WriteLine($"Average Order Value: ${avgOrderValue:C}");

// Inventory Value
var inventoryValue = await productService.GetInventoryValueAsync();
Console.WriteLine($"Total Inventory Value: ${inventoryValue:C}");

// Low Stock Alert
var lowStockProducts = await productService.GetLowStockProductsAsync(10);
Console.WriteLine($"Products with low stock: {lowStockProducts.Count}");
```

## 🔐 Exception Handling

### Custom Exception Types
```csharp
try
{
    var user = await userRepository.GetByIdAsync(999);
    if (user == null)
        throw new EntityNotFoundException(nameof(User), 999);
}
catch (EntityNotFoundException ex)
{
    logger.LogWarning(ex.Message);
}

try
{
    var order = await orderService.CreateOrderAsync(invalidDto);
}
catch (ValidationException ex)
{
    logger.LogWarning($"Validation Error: {ex.Message}");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value)}");
    }
}

try
{
    await authService.LoginAsync("user", "wrongpass");
}
catch (UnauthorizedAccessException ex)
{
    logger.LogWarning("Unauthorized access attempt");
}
```

## 📝 Database Queries

### Get User's Orders
```csharp
var orderRepository = serviceProvider.GetRequiredService<OrderRepository>();
var userOrders = await orderRepository.GetUserOrdersAsync(userId);
```

### Get Top-Rated Products
```csharp
var topProducts = await productService.GetTopRatedProductsAsync(10);
```

### Revenue by Date Range
```csharp
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 12, 31);
var yearRevenue = await orderService.GetRevenueByDateRangeAsync(startDate, endDate);
```

### Get Products by Category
```csharp
var electronicsProducts = await productService.GetProductsByCategoryAsync(1);
```

## 🧪 Testing the System

### Complete Flow Test
```csharp
// 1. Register user
var customer = await authService.RegisterAsync("test_user", "test@example.com", 
    "password123", "Test", "User", UserRole.Customer);

// 2. Create products
var product1 = await productService.CreateProductAsync(new CreateProductDto { /* ... */ });
var product2 = await productService.CreateProductAsync(new CreateProductDto { /* ... */ });

// 3. Create order
var order = await orderService.CreateOrderAsync(new CreateOrderDto
{
    UserId = customer.Id,
    Items = new List<OrderItemDto>
    {
        new OrderItemDto { ProductId = product1.Id, Quantity = 2 },
        new OrderItemDto { ProductId = product2.Id, Quantity = 1 }
    }
});

// 4. Process payment
var paymentProcessor = serviceProvider.GetRequiredService<PaymentProcessor>();
paymentProcessor.SetPaymentStrategy(new CreditCardPaymentStrategy(logger));
await paymentProcessor.PayAsync(order.TotalAmount, "card_details");

// 5. Update order status
await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing);
await orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Shipped);

Console.WriteLine("✅ Complete flow test passed!");
```

## 📚 File Reference

| File | Purpose |
|------|---------|
| `Models/` | Domain entities |
| `Repositories/` | Data access layer |
| `Services/` | Business logic layer |
| `Database/` | EF Core context & SQL scripts |
| `Patterns/` | Design pattern implementations |
| `DTOs/` | Data transfer objects |
| `Exceptions/` | Custom exception types |
| `Configuration/` | Dependency injection setup |

## 🐛 Troubleshooting

### Database Connection Error
```
Check connection string in Program.cs
Verify SQL Server is running
Check database name and credentials
```

### Entity Not Found
```
Ensure the entity exists in the database
Check the ID is correct
Verify the foreign keys are valid
```

### Validation Error
```
Review the DTO validation rules
Check required fields are provided
Validate numeric ranges and formats
```

## 📈 Extending the System

### Add New Payment Method
1. Create new class implementing `IPaymentStrategy`
2. Add to `Patterns/PaymentStrategies.cs`
3. Use with `PaymentProcessor.SetPaymentStrategy()`

### Add New Repository Methods
1. Extend specialized repository class
2. Add custom query methods
3. Update related services

### Add New Service
1. Create service class
2. Inject repositories
3. Register in `ServiceCollectionExtensions.cs`
4. Use via dependency injection

## 💼 Portfolio Tips

- **Highlight Architecture**: Explain your clean architecture decisions
- **Design Patterns**: Showcase pattern implementations
- **Database**: Discuss schema design and relationships
- **Error Handling**: Demonstrate comprehensive exception management
- **Security**: Mention password hashing and validation
- **Performance**: Explain caching and query optimization

---

**Happy Coding! 🎉**
