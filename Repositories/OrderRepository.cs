using Microsoft.EntityFrameworkCore;
using ECommerceSystem.Database;
using ECommerceSystem.Models;

namespace ECommerceSystem.Repositories;

/// <summary>
/// Order-specific repository with business queries
/// </summary>
public class OrderRepository : Repository<Order>
{
    public OrderRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _dbSet
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<int> GetTotalOrderCountAsync()
    {
        return await _dbSet.CountAsync(o => o.Status != OrderStatus.Cancelled);
    }

    public async Task<decimal> GetAverageOrderValueAsync()
    {
        var totalOrders = await GetTotalOrderCountAsync();
        if (totalOrders == 0) return 0;
        return await GetTotalRevenueAsync() / totalOrders;
    }
}
