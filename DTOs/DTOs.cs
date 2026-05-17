using ECommerceSystem.Models;

namespace ECommerceSystem.DTOs;

// Product DTOs
public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = null!;
    public int CategoryId { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public ProductStatus? Status { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public decimal Rating { get; set; }
}

// Order DTOs
public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    public int UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}

// User DTOs
public class UserRegistrationDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

public class UserLoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; }
}

// Review DTOs
public class CreateReviewDto
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class ReviewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Rating { get; set; }
    public string AuthorName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

// Category DTOs
public class CreateCategoryDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
