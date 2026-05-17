using Microsoft.EntityFrameworkCore;
using ECommerceSystem.Database;
using ECommerceSystem.Models;

namespace ECommerceSystem.Repositories;

/// <summary>
/// User-specific repository with custom queries
/// </summary>
public class UserRepository : Repository<User>
{
    public UserRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetByRoleAsync(UserRole role)
    {
        return await _dbSet.Where(u => u.Role == role && u.IsActive).ToListAsync();
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<int> GetUserCountByRoleAsync(UserRole role)
    {
        return await _dbSet.CountAsync(u => u.Role == role && u.IsActive);
    }
}
