using Microsoft.EntityFrameworkCore;
using TopDom.Data;
using TopDom.Models;

namespace TopDom.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> HasProductsAsync(int categoryId) =>
        await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
}