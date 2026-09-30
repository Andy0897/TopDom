using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Categories;

namespace TopDom.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<List<Category>> GetAllAsync() => await _categories.GetAllAsync();

    public async Task<Category?> GetByIdAsync(int id) => await _categories.GetByIdAsync(id);

    public async Task CreateAsync(CategoryFormViewModel model)
    {
        var category = new Category { Name = model.Name.Trim() };
        await _categories.AddAsync(category);
        await _categories.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoryFormViewModel model)
    {
        var category = await _categories.GetByIdAsync(model.Id)
            ?? throw new Exception("Категорията не е намерена.");

        category.Name = model.Name.Trim();
        _categories.Update(category);
        await _categories.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        if (await _categories.HasProductsAsync(id))
            throw new Exception("Категорията не може да бъде изтрита, защото има продукти в нея.");

        var category = await _categories.GetByIdAsync(id)
            ?? throw new Exception("Категорията не е намерена.");

        _categories.Delete(category);
        await _categories.SaveChangesAsync();
    }
}