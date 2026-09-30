using TopDom.Models;

namespace TopDom.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> SearchAsync(string? search, int? categoryId, int page, int pageSize);
    Task<int> CountAsync(string? search, int? categoryId);
    Task<Product?> GetWithCategoryAsync(int id);
}