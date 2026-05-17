namespace ECommerceSystem.Models;

public enum UserRole
{
    Admin = 1,
    Customer = 2,
    Vendor = 3
}

/// <summary>
/// Represents a user in the system with role-based access
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public string GetFullName() => $"{FirstName} {LastName}";

    public bool ValidatePassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }
}
