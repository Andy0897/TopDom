using Microsoft.EntityFrameworkCore;
using TopDom.Data;
using TopDom.Models;

namespace TopDom.Repositories;

public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ApplicationUser?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> EmailExistsAsync(string email) =>
        await _dbSet.AnyAsync(u => u.Email == email);

    public async Task<bool> AnyAdminAsync() =>
        await _dbSet.AnyAsync(u => u.Role == "Admin");
}