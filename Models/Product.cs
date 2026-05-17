namespace ECommerceSystem.Models;

public enum ProductStatus
{
    Active = 1,
    Inactive = 2,
    Discontinued = 3
}

/// <summary>
/// Represents a product in the e-commerce system
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string SKU { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public bool IsAvailable() => StockQuantity > 0 && Status == ProductStatus.Active;

    public bool CanDeductStock(int quantity) => StockQuantity >= quantity;

    public void DeductStock(int quantity)
    {
        if (!CanDeductStock(quantity))
            throw new InvalidOperationException("Insufficient stock");
        StockQuantity -= quantity;
    }

    public void RestockProduct(int quantity)
    {
        StockQuantity += quantity;
    }
}
