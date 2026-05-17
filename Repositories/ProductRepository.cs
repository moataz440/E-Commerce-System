using Microsoft.EntityFrameworkCore;
using ECommerceSystem.Database;
using ECommerceSystem.Models;

namespace ECommerceSystem.Repositories;

/// <summary>
/// Product-specific repository with business queries
/// </summary>
public class ProductRepository : Repository<Product>
{
    public ProductRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && p.Status == ProductStatus.Active)
            .ToListAsync();
    }

    public async Task<List<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= threshold && p.Status == ProductStatus.Active)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _dbSet
            .Where(p => (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                        && p.Status == ProductStatus.Active)
            .ToListAsync();
    }

    public async Task<List<Product>> GetTopRatedProductsAsync(int count = 10)
    {
        return await _dbSet
            .Where(p => p.Status == ProductStatus.Active)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalInventoryValueAsync()
    {
        return await _dbSet
            .Where(p => p.Status == ProductStatus.Active)
            .SumAsync(p => p.Price * p.StockQuantity);
    }
}
