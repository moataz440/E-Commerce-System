namespace ECommerceSystem.Models;

/// <summary>
/// Represents a product category
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
