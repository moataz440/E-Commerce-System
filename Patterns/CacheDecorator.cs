namespace ECommerceSystem.Patterns;

/// <summary>
/// Decorator pattern for caching
/// </summary>
public interface ICacheDecorator<T>
{
    Task<T?> GetOrExecuteAsync(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class InMemoryCacheDecorator<T> : ICacheDecorator<T>
{
    private readonly Dictionary<string, CacheEntry<T>> _cache = new();
    private readonly ILogger<InMemoryCacheDecorator<T>> _logger;

    public InMemoryCacheDecorator(ILogger<InMemoryCacheDecorator<T>> logger)
    {
        _logger = logger;
    }

    public async Task<T?> GetOrExecuteAsync(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogInformation($"Cache hit for key: {key}");
                return entry.Value;
            }
            else
            {
                _cache.Remove(key);
                _logger.LogInformation($"Cache expired for key: {key}");
            }
        }

        _logger.LogInformation($"Cache miss for key: {key}, executing factory");
        var result = await factory();

        _cache[key] = new CacheEntry<T>
        {
            Value = result,
            ExpiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(30))
        };

        return result;
    }

    private class CacheEntry<TValue>
    {
        public TValue? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
