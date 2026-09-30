using TopDom.Models;
using TopDom.ViewModels.Products;

namespace TopDom.Services.Interfaces;

public interface IProductService
{
    Task<ProductListViewModel> GetListAsync(string? search, int? categoryId, int page);
    Task<Product?> GetWithCategoryAsync(int id);
    Task CreateAsync(ProductFormViewModel model);
    Task UpdateAsync(ProductFormViewModel model);
    Task DeleteAsync(int id);
}