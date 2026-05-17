using ECommerceSystem.Models;
using ECommerceSystem.Repositories;
using ECommerceSystem.Exceptions;
using ECommerceSystem.DTOs;

namespace ECommerceSystem.Services;

/// <summary>
/// Order service with business logic
/// </summary>
public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(OrderRepository orderRepository, ProductRepository productRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Order must contain at least one item");

        var order = new Order
        {
            UserId = dto.UserId,
            OrderNumber = GenerateOrderNumber(),
            ShippingAddress = dto.ShippingAddress,
            Notes = dto.Notes,
            Status = OrderStatus.Pending
        };

        decimal totalAmount = 0;

        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new EntityNotFoundException(nameof(Product), item.ProductId);

            if (!product.CanDeductStock(item.Quantity))
                throw new InvalidOperationException($"Insufficient stock for {product.Name}");

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.GetLineTotal();

            // Deduct stock
            product.DeductStock(item.Quantity);
            await _productRepository.UpdateAsync(product);
        }

        order.TotalAmount = totalAmount;
        var result = await _orderRepository.AddAsync(order);
        _logger.LogInformation($"Order created: {order.OrderNumber}, Total: ${totalAmount}");
        return result;
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new EntityNotFoundException(nameof(Order), orderId);

        order.UpdateStatus(newStatus);
        var result = await _orderRepository.UpdateAsync(order);
        _logger.LogInformation($"Order status updated: {order.OrderNumber}, Status: {newStatus}");
        return result;
    }

    public async Task<Order?> GetOrderAsync(int id)
    {
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        return await _orderRepository.GetUserOrdersAsync(userId);
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _orderRepository.GetTotalRevenueAsync();
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _orderRepository.GetRevenueByDateRangeAsync(startDate, endDate);
    }

    public async Task<int> GetTotalOrderCountAsync()
    {
        return await _orderRepository.GetTotalOrderCountAsync();
    }

    public async Task<decimal> GetAverageOrderValueAsync()
    {
        return await _orderRepository.GetAverageOrderValueAsync();
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new EntityNotFoundException(nameof(Order), orderId);

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel delivered or already cancelled orders");

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            if (item.Product != null)
            {
                item.Product.RestockProduct(item.Quantity);
                await _productRepository.UpdateAsync(item.Product);
            }
        }

        order.UpdateStatus(OrderStatus.Cancelled);
        await _orderRepository.UpdateAsync(order);
        _logger.LogInformation($"Order cancelled: {order.OrderNumber}");
        return true;
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
    }
}
