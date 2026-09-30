using Microsoft.EntityFrameworkCore;
using TopDom.Data;
using TopDom.Models;

namespace TopDom.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    private IQueryable<Product> Filtered(string? search, int? categoryId)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        return query;
    }

    public async Task<List<Product>> SearchAsync(string? search, int? categoryId, int page, int pageSize)
    {
        return await Filtered(search, categoryId)
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new Product   // ImageData НЕ се тегли тук (виж бележката по-долу)
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                Category = p.Category,
                ImageContentType = p.ImageContentType
            })
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search, int? categoryId) =>
        await Filtered(search, categoryId).CountAsync();

    public async Task<Product?> GetWithCategoryAsync(int id) =>
        await _dbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
}