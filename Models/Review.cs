namespace ECommerceSystem.Models;

/// <summary>
/// Represents a product review
/// </summary>
public class Review : BaseEntity
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // 1-5
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int HelpfulCount { get; set; }

    // Navigation properties
    public Product? Product { get; set; }
    public User? User { get; set; }

    public bool IsValidRating() => Rating >= 1 && Rating <= 5;
}
