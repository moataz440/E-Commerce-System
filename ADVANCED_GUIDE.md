# Advanced Implementation Guide

## 🔧 Extending the System

### Adding a New Payment Method

**Step 1**: Create new strategy class
```csharp
// File: Patterns/PaymentStrategies.cs - Add this class

public class ApplePayPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<ApplePayPaymentStrategy> _logger;

    public ApplePayPaymentStrategy(ILogger<ApplePayPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails)
    {
        _logger.LogInformation($"Processing Apple Pay payment: ${amount}");
        // Integration with Apple Pay API
        await Task.Delay(700);
        return true;
    }

    public async Task<bool> RefundAsync(decimal amount, string transactionId)
    {
        _logger.LogInformation($"Processing Apple Pay refund: ${amount}");
        await Task.Delay(700);
        return true;
    }

    public string GetPaymentMethodName() => "Apple Pay";
}
```

**Step 2**: Use in application
```csharp
var paymentProcessor = serviceProvider.GetRequiredService<PaymentProcessor>();
paymentProcessor.SetPaymentStrategy(new ApplePayPaymentStrategy(logger));
await paymentProcessor.PayAsync(99.99m, applePayToken);
```

---

### Adding a New Repository Method

**Step 1**: Add method to repository
```csharp
// File: Repositories/ProductRepository.cs - Add this method

public async Task<List<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice)
{
    return await _dbSet
        .Where(p => p.Price >= minPrice && p.Price <= maxPrice && p.Status == ProductStatus.Active)
        .OrderBy(p => p.Price)
        .ToListAsync();
}
```

**Step 2**: Use in service
```csharp
// File: Services/ProductService.cs - Add this method

public async Task<List<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice)
{
    if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
        throw new ValidationException("Invalid price range");
    
    return await _productRepository.GetProductsInPriceRangeAsync(minPrice, maxPrice);
}
```

---

### Creating an Advanced Query with Include

**Complex order retrieval**:
```csharp
// File: Repositories/OrderRepository.cs - Add method

public async Task<Order?> GetDetailedOrderAsync(int orderId)
{
    return await _dbSet
        .Include(o => o.User)
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .ThenInclude(p => p.Category)
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .ThenInclude(p => p.Reviews)
        .FirstOrDefaultAsync(o => o.Id == orderId);
}
```

---

### Implementing Pagination

**Add to repository base**:
```csharp
// File: Repositories/Repository.cs - Add method

public async Task<(List<T> items, int totalCount)> GetPagedAsync(
    int pageNumber, int pageSize, Func<IQueryable<T>, IQueryable<T>>? filter = null)
{
    var query = _dbSet.AsQueryable();
    query = filter?.Invoke(query) ?? query;
    
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return (items, totalCount);
}
```

**Usage**:
```csharp
var (products, total) = await productRepository.GetPagedAsync(
    pageNumber: 1,
    pageSize: 10,
    filter: q => q.Where(p => p.Status == ProductStatus.Active)
);

Console.WriteLine($"Total products: {total}");
Console.WriteLine($"Page 1 products: {products.Count}");
```

---

### Adding Advanced Filtering

**Create filter DTO**:
```csharp
// File: DTOs/DTOs.cs - Add this class

public class ProductFilterDto
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinRating { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

**Add repository method**:
```csharp
// File: Repositories/ProductRepository.cs

public async Task<(List<Product> products, int total)> GetFilteredProductsAsync(ProductFilterDto filter)
{
    var query = _dbSet.Where(p => p.Status == ProductStatus.Active);
    
    if (filter.MinPrice.HasValue)
        query = query.Where(p => p.Price >= filter.MinPrice);
    
    if (filter.MaxPrice.HasValue)
        query = query.Where(p => p.Price <= filter.MaxPrice);
    
    if (filter.CategoryId.HasValue)
        query = query.Where(p => p.CategoryId == filter.CategoryId);
    
    if (filter.MinRating.HasValue)
        query = query.Where(p => p.Rating >= filter.MinRating);
    
    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        query = query.Where(p => p.Name.Contains(filter.SearchTerm) || 
                                p.Description.Contains(filter.SearchTerm));
    
    var total = await query.CountAsync();
    var products = await query
        .OrderByDescending(p => p.Rating)
        .Skip((filter.Page - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToListAsync();
    
    return (products, total);
}
```

**Usage**:
```csharp
var filter = new ProductFilterDto
{
    MinPrice = 100,
    MaxPrice = 500,
    CategoryId = 1,
    SearchTerm = "laptop",
    Page = 1,
    PageSize = 20
};

var (products, total) = await productRepository.GetFilteredProductsAsync(filter);
```

---

### Implementing Unit of Work Pattern

**Create Unit of Work interface**:
```csharp
// File: Interfaces/IUnitOfWork.cs

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Product> Products { get; }
    IRepository<Order> Orders { get; }
    
    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitAsync();
    Task<bool> RollbackAsync();
}
```

**Implementation**:
```csharp
// File: Repositories/UnitOfWork.cs

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private IRepository<User>? _users;
    private IRepository<Product>? _products;
    private IRepository<Order>? _orders;

    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    
    public async Task<bool> BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
        return true;
    }

    public async Task<bool> CommitAsync()
    {
        await _context.Database.CommitTransactionAsync();
        return true;
    }

    public async Task<bool> RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
        return true;
    }

    public void Dispose() => _context.Dispose();
}
```

**Usage**:
```csharp
using (var unitOfWork = new UnitOfWork(context))
{
    var product = new Product { /* ... */ };
    await unitOfWork.Products.AddAsync(product);
    
    var user = new User { /* ... */ };
    await unitOfWork.Users.AddAsync(user);
    
    await unitOfWork.SaveChangesAsync();
}
```

---

### Advanced Caching with TTL Management

**Enhanced cache decorator**:
```csharp
// File: Patterns/CacheDecorator.cs - Add this method

public async Task InvalidateAsync(string key)
{
    if (_cache.ContainsKey(key))
    {
        _cache.Remove(key);
        _logger.LogInformation($"Cache invalidated for key: {key}");
    }
}

public async Task<int> GetCacheSizeAsync()
{
    return _cache.Count;
}

public async Task ClearAllAsync()
{
    _cache.Clear();
    _logger.LogInformation("All cache cleared");
}
```

**Usage**:
```csharp
var cache = new InMemoryCacheDecorator<Product>(logger);

// Get with caching
var product = await cache.GetOrExecuteAsync("product-1", 
    () => repository.GetByIdAsync(1),
    TimeSpan.FromHours(1));

// Invalidate on update
await cache.InvalidateAsync("product-1");

// Clear all when needed
await cache.ClearAllAsync();
```

---

### Implementing Repository Projection with AutoMapper

**Install AutoMapper**:
```xml
<PackageReference Include="AutoMapper" Version="13.0.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.0" />
```

**Create mappings**:
```csharp
// File: Configuration/AutoMapperProfile.cs

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()));
        
        CreateMap<Product, ProductDto>();
        
        CreateMap<Order, OrderDto>();
        
        CreateMap<CreateProductDto, Product>();
    }
}
```

**Update Program.cs**:
```csharp
services.AddAutoMapper(typeof(AutoMapperProfile));
```

**Use in services**:
```csharp
private readonly IMapper _mapper;

public async Task<UserDto> GetUserAsync(int id)
{
    var user = await repository.GetByIdAsync(id);
    return _mapper.Map<UserDto>(user);
}
```

---

### Batch Operations

**Add to repository**:
```csharp
// File: Repositories/Repository.cs - Add method

public async Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> entities)
{
    await _dbSet.AddRangeAsync(entities);
    await _context.SaveChangesAsync();
    return entities;
}

public async Task<bool> UpdateManyAsync(IEnumerable<T> entities)
{
    _dbSet.UpdateRange(entities);
    await _context.SaveChangesAsync();
    return true;
}

public async Task<bool> DeleteManyAsync(IEnumerable<int> ids)
{
    var entities = await _dbSet.Where(e => ids.Contains(EF.Property<int>(e, "Id")))
        .ToListAsync();
    _dbSet.RemoveRange(entities);
    await _context.SaveChangesAsync();
    return true;
}
```

**Usage**:
```csharp
var products = new List<Product> { /* ... */ };
await productRepository.AddManyAsync(products);

var idsToDelete = new List<int> { 1, 2, 3 };
await productRepository.DeleteManyAsync(idsToDelete);
```

---

### Implementing Specification Pattern

**Create specification base class**:
```csharp
// File: Repositories/Specifications/Specification.cs

public abstract class Specification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
}

public class ProductSpecification : Specification<Product>
{
    public ProductSpecification(int categoryId)
    {
        Criteria = p => p.CategoryId == categoryId && p.Status == ProductStatus.Active;
        OrderByDescending = p => p.Rating;
        AddInclude(p => p.Category);
    }
}
```

---

### Error Recovery and Retry Logic

**Add retry policy**:
```csharp
// File: Utils/RetryPolicy.cs

public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;
    private readonly ILogger _logger;

    public RetryPolicy(int maxRetries = 3, int delayMs = 100, ILogger? logger = null)
    {
        _maxRetries = maxRetries;
        _delay = TimeSpan.FromMilliseconds(delayMs);
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        for (int i = 0; i < _maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < _maxRetries - 1)
            {
                _logger?.LogWarning($"Attempt {i + 1} failed: {ex.Message}. Retrying...");
                await Task.Delay(_delay);
            }
        }

        throw new InvalidOperationException($"Operation failed after {_maxRetries} attempts");
    }
}
```

**Usage**:
```csharp
var retryPolicy = new RetryPolicy(maxRetries: 3, delayMs: 100, logger: logger);

var product = await retryPolicy.ExecuteAsync(() => 
    repository.GetByIdAsync(productId)
);
```

---

## 🧪 Testing Patterns

### Mock Repository Pattern

```csharp
// For Unit Testing

public class MockProductRepository : IRepository<Product>
{
    private readonly List<Product> _products = new();

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }

    public async Task<Product> AddAsync(Product entity)
    {
        entity.Id = _products.Count + 1;
        _products.Add(entity);
        return await Task.FromResult(entity);
    }

    // Implement other methods...
}

// Usage in tests
[TestMethod]
public async Task CreateProduct_WithValidData_ReturnsProduct()
{
    // Arrange
    var mockRepository = new MockProductRepository();
    var service = new ProductService(mockRepository, mockLogger);

    // Act
    var product = await service.CreateProductAsync(validDto);

    // Assert
    Assert.IsNotNull(product);
    Assert.AreEqual("Test Product", product.Name);
}
```

---

## 📊 Monitoring & Analytics

### Query Performance Logging

```csharp
// File: Utils/PerformanceLogger.cs

public class PerformanceLogger
{
    private readonly ILogger _logger;

    public PerformanceLogger(ILogger logger) => _logger = logger;

    public async Task<T> LogExecutionTimeAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await operation();
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation($"Operation '{operationName}' completed in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
```

---

This guide covers advanced patterns and techniques for extending and maintaining the E-Commerce system.
