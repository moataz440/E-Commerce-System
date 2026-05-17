# Architecture & Design Patterns Documentation

## 🏛️ System Architecture

### Layered Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                │
│              (Console App - Program.cs)             │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│                   SERVICE LAYER                     │
│  AuthenticationService │ ProductService │ OrderService
│         + Business Logic                            │
│         + Validation                                │
│         + Coordination                              │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│               REPOSITORY LAYER                      │
│  Repository<T> │ UserRepository │ ProductRepository │
│         + Data Access Abstraction                   │
│         + Query Methods                             │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│                 DATA LAYER                          │
│         Entity Framework Core DbContext             │
│                  ↓                                  │
│              SQL Server Database                    │
└─────────────────────────────────────────────────────┘
```

## 🎨 OOP Principles Implementation

### 1. ENCAPSULATION
**Definition**: Hiding internal implementation details and exposing only necessary interfaces

**Example: Product.cs**
```csharp
public class Product : BaseEntity
{
    private decimal _price;
    
    public decimal Price 
    { 
        get => _price; 
        private set => _price = value; 
    }
    
    public bool CanDeductStock(int quantity) => StockQuantity >= quantity;
    
    public void DeductStock(int quantity)
    {
        if (!CanDeductStock(quantity))
            throw new InvalidOperationException("Insufficient stock");
        StockQuantity -= quantity;
    }
}
```

**Benefits**:
- Business logic is protected from outside modification
- Validation happens at class level
- Prevents invalid states

### 2. INHERITANCE
**Definition**: Creating class hierarchies to promote code reuse

**Example: User Hierarchy**
```
     User (Base)
      ├── Admin
      ├── Customer
      └── Vendor
```

**Implementation**:
```csharp
public abstract class User : BaseEntity
{
    public string Username { get; protected set; }
    public virtual bool Login(string username, string password) { }
    public abstract void ViewDashboard();
}

public class Customer : User
{
    public ICollection<Order> Orders { get; set; }
    public override void ViewDashboard() { /* customer dashboard */ }
}
```

**Benefits**:
- Code reuse through shared base class
- Polymorphic behavior
- Clear role hierarchy

### 3. POLYMORPHISM
**Definition**: Objects can take multiple forms and respond differently to same messages

**Example: Repository Pattern**
```csharp
public interface IRepository<T> { }

public class Repository<T> : IRepository<T> { }
public class UserRepository : Repository<User> { }
public class ProductRepository : Repository<Product> { }
```

**Benefits**:
- Same interface, different implementations
- Easy to extend without modifying existing code
- Supports Liskov Substitution Principle

### 4. ABSTRACTION
**Definition**: Hiding complexity and showing only relevant features

**Example: Payment Strategy**
```csharp
public abstract class PaymentMethod
{
    public abstract Task<bool> ProcessPaymentAsync(decimal amount);
}

public class CreditCardPaymentStrategy : PaymentMethod
{
    public override async Task<bool> ProcessPaymentAsync(decimal amount)
    {
        // Complex credit card processing hidden
    }
}
```

**Benefits**:
- Clients don't need to know implementation details
- Easy to swap implementations
- Cleaner code at usage level

## 🎯 Design Patterns

### 1. REPOSITORY PATTERN

**Problem**: Tight coupling between business logic and data access

**Solution**: Abstract data access behind interfaces

```
┌─────────────────────┐
│   Service Layer     │
└──────────┬──────────┘
           │ depends on
┌──────────▼──────────┐
│   IRepository<T>    │
└──────────┬──────────┘
           │ implements
┌──────────▼──────────┐
│ Repository<T>       │
│ UserRepository      │
│ ProductRepository   │
└──────────┬──────────┘
           │ uses
┌──────────▼──────────────┐
│ EF Core DbContext       │
└─────────────────────────┘
```

**Code Example**:
```csharp
// Service depends on abstraction
public class ProductService
{
    private readonly ProductRepository _productRepository;
    
    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        return await _productRepository.AddAsync(product);
    }
}

// Can swap implementation without changing service
IRepository<Product> repository = new ProductRepository(context);
```

**Benefits**:
- Loose coupling
- Easy to test (mock repositories)
- Easy to swap database implementations

### 2. STRATEGY PATTERN

**Problem**: Need multiple algorithms for payment processing

**Solution**: Encapsulate each algorithm as a separate strategy

```
┌──────────────────────────┐
│  PaymentProcessor        │
│  (uses strategy)         │
└──────────────┬───────────┘
               │
       ┌───────┴─────────┬─────────────────┐
       │                 │                 │
┌──────▼─────────┐ ┌──────▼──────┐ ┌──────▼────────┐
│ CreditCard     │ │  PayPal     │ │   Stripe      │
│ Strategy       │ │ Strategy    │ │  Strategy     │
└────────────────┘ └─────────────┘ └───────────────┘
```

**Code Example**:
```csharp
public interface IPaymentStrategy
{
    Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails);
}

public class PaymentProcessor
{
    private IPaymentStrategy _strategy;
    
    public void SetPaymentStrategy(IPaymentStrategy strategy) => _strategy = strategy;
    
    public async Task<bool> PayAsync(decimal amount, string details)
    {
        return await _strategy.ProcessPaymentAsync(amount, details);
    }
}

// Usage
paymentProcessor.SetPaymentStrategy(new CreditCardPaymentStrategy());
await paymentProcessor.PayAsync(99.99m, cardDetails);

// Switch at runtime
paymentProcessor.SetPaymentStrategy(new PayPalPaymentStrategy());
await paymentProcessor.PayAsync(99.99m, paypalDetails);
```

**Benefits**:
- Easy to add new payment methods
- No modification to existing strategies
- Runtime strategy switching
- Clean Open/Closed Principle

### 3. DECORATOR PATTERN

**Problem**: Need to add caching without modifying repository logic

**Solution**: Wrap objects with additional functionality

```
┌─────────────────────────────────────┐
│ InMemoryCacheDecorator<T>           │
│ ┌───────────────────────────────┐   │
│ │ Inner: IRepository<T>         │   │
│ │                               │   │
│ │ GetOrExecuteAsync():          │   │
│ │ 1. Check cache                │   │
│ │ 2. If miss, call factory      │   │
│ │ 3. Store in cache             │   │
│ │ 4. Return result              │   │
│ └───────────────────────────────┘   │
└─────────────────────────────────────┘
```

**Code Example**:
```csharp
public interface ICacheDecorator<T>
{
    Task<T?> GetOrExecuteAsync(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class InMemoryCacheDecorator<T> : ICacheDecorator<T>
{
    private readonly Dictionary<string, CacheEntry<T>> _cache = new();
    
    public async Task<T?> GetOrExecuteAsync(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        // Check cache
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
            return entry.Value;
        
        // Execute factory function
        var result = await factory();
        
        // Store in cache with expiration
        _cache[key] = new CacheEntry<T> 
        { 
            Value = result, 
            ExpiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(30))
        };
        
        return result;
    }
}

// Usage
var cache = new InMemoryCacheDecorator<Product>(logger);
var product = await cache.GetOrExecuteAsync(
    "product-1",
    () => repository.GetByIdAsync(1),
    TimeSpan.FromHours(1)
);
```

**Benefits**:
- Add functionality without modifying original class
- Transparent to clients
- Can combine multiple decorators
- Follows Single Responsibility Principle

### 4. FACTORY PATTERN

**Problem**: Creating payment strategies based on type

**Solution**: Centralize object creation

```csharp
public static class PaymentStrategyFactory
{
    public static IPaymentStrategy Create(string type)
    {
        return type.ToLower() switch
        {
            "creditcard" => new CreditCardPaymentStrategy(logger),
            "paypal" => new PayPalPaymentStrategy(logger),
            "stripe" => new StripePaymentStrategy(logger),
            _ => throw new ArgumentException($"Unknown payment type: {type}")
        };
    }
}

// Usage
var strategy = PaymentStrategyFactory.Create("creditcard");
paymentProcessor.SetPaymentStrategy(strategy);
```

### 5. SINGLETON PATTERN

**Problem**: Logger configuration should have single instance

**Solution**: Ensure only one instance exists

```csharp
public static class LoggingConfiguration
{
    private static readonly Lazy<ILogger> _logger = 
        new Lazy<ILogger>(() => CreateLogger());
    
    public static ILogger Logger => _logger.Value;
    
    private static ILogger CreateLogger()
    {
        return new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
```

### 6. DEPENDENCY INJECTION PATTERN

**Problem**: Hard-coded dependencies make testing difficult

**Solution**: Inject dependencies from outside

```csharp
// Configuration
public static IServiceCollection AddECommerceServices(this IServiceCollection services, string connectionString)
{
    services.AddDbContext<ECommerceDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    services.AddScoped<ProductRepository>();
    services.AddScoped<ProductService>();
    
    return services;
}

// Usage
var services = new ServiceCollection();
services.AddECommerceServices(connectionString);
var serviceProvider = services.BuildServiceProvider();

var productService = serviceProvider.GetRequiredService<ProductService>();
```

**Benefits**:
- Loose coupling
- Easy to test (mock dependencies)
- Follows Inversion of Control principle

## 📊 Database Design

### Entity Relationship Diagram
```
┌─────────────┐
│   Users     │
│─────────────│
│ Id (PK)     │
│ Username    │
│ Email       │
│ Role        │
│ IsActive    │
└─────────────┘
      │
      │ 1:M
      │
┌─────▼──────────┐        ┌──────────────┐
│   Orders       │        │  Categories  │
│────────────────│        │──────────────│
│ Id (PK)        │        │ Id (PK)      │
│ UserId (FK)    │        │ Name         │
│ OrderNumber    │        │ Description  │
│ Status         │        └──────────────┘
│ TotalAmount    │              │
│ OrderDate      │              │ 1:M
└─────┬──────────┘              │
      │                    ┌─────▼─────────┐
      │ 1:M              │    Products    │
      │            ┌─────►────────────────┤
      │            │     │ Id (PK)        │
┌─────▼────────────┴──┐  │ CategoryId(FK) │
│   OrderItems       │  │ SKU            │
│───────────────────┤  │ Price          │
│ Id (PK)           │  │ StockQuantity  │
│ OrderId (FK)      │  │ Rating         │
│ ProductId (FK) ───┴─►│ Description    │
│ Quantity          │  └────────┬────────┘
│ UnitPrice         │           │
└───────────────────┘           │ 1:M
                        ┌───────▼──────────┐
                        │    Reviews       │
                        │──────────────────│
                        │ Id (PK)          │
                        │ ProductId (FK)   │
                        │ UserId (FK)      │
                        │ Rating           │
                        │ Title            │
                        │ Content          │
                        └──────────────────┘
```

### Relationships
- **User → Orders**: 1:Many (One user has many orders)
- **User → Reviews**: 1:Many (One user writes many reviews)
- **Product → OrderItems**: 1:Many
- **Product → Reviews**: 1:Many
- **Order → OrderItems**: 1:Many
- **Category → Products**: 1:Many

## 🔄 Data Flow Examples

### Order Creation Flow
```
1. Client calls OrderService.CreateOrderAsync()
                    ↓
2. Service validates DTO and items
                    ↓
3. Service checks product availability (ProductRepository)
                    ↓
4. Service deducts stock from products
                    ↓
5. Service creates Order with OrderItems
                    ↓
6. Service saves to database (OrderRepository)
                    ↓
7. Service logs operation
                    ↓
8. Return created Order
```

### Payment Processing Flow
```
1. Client selects payment method
                    ↓
2. Create appropriate strategy (Factory)
                    ↓
3. Set strategy in PaymentProcessor
                    ↓
4. PaymentProcessor.PayAsync() called
                    ↓
5. Strategy.ProcessPaymentAsync() executes
                    ↓
6. Update order status
                    ↓
7. Log transaction
                    ↓
8. Return success/failure
```

### Caching Flow
```
1. Request to get product (key: "product-1")
                    ↓
2. CacheDecorator checks memory
                    ↓
        ┌─── Cache HIT ────┐
        │                   │
        ├─ Check expiration ├─ Expired → Clear cache
        │                   │
        └─ Return value ────┘
                    │
        ┌─── Cache MISS ────┐
        │                   │
        ├─ Call factory ────┤
        │ (Repository call)  │
        │                   │
        ├─ Store in cache ──┤
        │ (with expiration)  │
        │                   │
        └─ Return value ────┘
```

## 🎓 SOLID Principles Application

### S - Single Responsibility
```csharp
// ✅ Good - Each class has one responsibility
public class OrderService { }        // Manages orders
public class ProductService { }      // Manages products
public class AuthenticationService {} // Manages auth

// ❌ Bad - Multiple responsibilities
public class SystemService 
{
    public void CreateOrder() { }
    public void CreateProduct() { }
    public void AuthenticateUser() { }
}
```

### O - Open/Closed
```csharp
// ✅ Open for extension, closed for modification
public interface IPaymentStrategy
{
    Task<bool> ProcessPaymentAsync(decimal amount);
}

public class NewPaymentMethodStrategy : IPaymentStrategy
{
    public async Task<bool> ProcessPaymentAsync(decimal amount) { }
}
// No need to modify existing code
```

### L - Liskov Substitution
```csharp
// ✅ Can substitute implementations
IRepository<Product> repo1 = new Repository<Product>(context);
IRepository<Product> repo2 = new ProductRepository(context);
// Both can be used interchangeably
```

### I - Interface Segregation
```csharp
// ✅ Specific interfaces
public interface IPaymentStrategy
{
    Task<bool> ProcessPaymentAsync(decimal amount);
}

// ❌ Too broad
public interface IPaymentGateway
{
    Task<bool> ProcessPaymentAsync();
    Task<bool> RefundAsync();
    Task<string> GetTransactionHistoryAsync();
    Task<bool> ValidateCardAsync();
}
```

### D - Dependency Inversion
```csharp
// ✅ Depend on abstraction
public class OrderService
{
    private readonly IRepository<Order> _repository;
    public OrderService(IRepository<Order> repository)
    {
        _repository = repository;
    }
}

// ❌ Depend on concrete implementation
public class OrderService
{
    private readonly OrderRepository _repository = new OrderRepository();
}
```

## 🚀 Performance Considerations

### Query Optimization
```csharp
// ✅ Load related data in single query
var orders = await _context.Orders
    .Include(o => o.OrderItems)
    .ThenInclude(oi => oi.Product)
    .Where(o => o.UserId == userId)
    .ToListAsync();

// ❌ N+1 query problem
var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
foreach (var order in orders)
{
    var items = await _context.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync();
}
```

### Caching Strategy
```csharp
// Cache frequently accessed, slowly changing data
var cachedProducts = await cache.GetOrExecuteAsync(
    "top-products",
    () => productRepository.GetTopRatedProductsAsync(10),
    TimeSpan.FromHours(1) // Cache for 1 hour
);
```

### Database Indexes
```sql
-- Indexes for frequently queried columns
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Products_SKU ON Products(SKU);
```

---

**This architecture demonstrates enterprise-level design principles suitable for real company projects.**
