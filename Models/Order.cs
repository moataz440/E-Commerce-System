namespace ECommerceSystem.Models;

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

/// <summary>
/// Represents a customer order
/// </summary>
public class Order : BaseEntity
{
    public int UserId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public decimal CalculateTotalAmount()
    {
        return OrderItems.Sum(x => x.Quantity * x.UnitPrice);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Shipped)
            ShippedDate = DateTime.UtcNow;
        else if (newStatus == OrderStatus.Delivered)
            DeliveredDate = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents an item in an order
/// </summary>
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }

    public decimal GetLineTotal() => Quantity * UnitPrice;
}
