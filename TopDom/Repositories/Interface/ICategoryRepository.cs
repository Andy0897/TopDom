using TopDom.Models;

namespace TopDom.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> HasProductsAsync(int categoryId);
}