using ECommerceSystem.Models;
using ECommerceSystem.Repositories;
using ECommerceSystem.Exceptions;

namespace ECommerceSystem.Services;

/// <summary>
/// Authentication service with password hashing
/// </summary>
public class AuthenticationService
{
    private readonly UserRepository _userRepository;

    public AuthenticationService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> RegisterAsync(string username, string email, string password, 
        string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            throw new ValidationException("Username must be at least 3 characters");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ValidationException("Invalid email format");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ValidationException("Password must be at least 6 characters");

        // Check if user exists
        if (await _userRepository.UserExistsAsync(username))
            throw new ValidationException("Username already exists");

        var existingEmail = await _userRepository.GetByEmailAsync(email);
        if (existingEmail != null)
            throw new ValidationException("Email already exists");

        // Create user with hashed password
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true
        };

        return await _userRepository.AddAsync(user);
    }

    public async Task<User> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid username or password");

        if (!user.ValidatePassword(password))
            throw new UnauthorizedAccessException("Invalid username or password");

        return user;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new EntityNotFoundException(nameof(User), userId);

        if (!user.ValidatePassword(currentPassword))
            throw new UnauthorizedAccessException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }
}
