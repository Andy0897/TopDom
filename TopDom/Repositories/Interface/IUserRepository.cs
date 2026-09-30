using TopDom.Models;

namespace TopDom.Repositories;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> AnyAdminAsync();
}