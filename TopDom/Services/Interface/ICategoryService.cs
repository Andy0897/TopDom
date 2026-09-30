using TopDom.Models;
using TopDom.ViewModels.Categories;

namespace TopDom.Services.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task CreateAsync(CategoryFormViewModel model);
    Task UpdateAsync(CategoryFormViewModel model);
    Task DeleteAsync(int id);
}