using TopDom.Repositories;
using TopDom.Services;
using TopDom.ViewModels.Categories;

namespace TopDom.Tests;

public class CategoryServiceTests
{
    private static CategoryService CreateService(TopDom.Data.ApplicationDbContext context) =>
        new(new CategoryRepository(context));

    [Fact]
    public async Task CreateAsync_SavesTrimmedCategory()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await service.CreateAsync(new CategoryFormViewModel { Name = "  Хладилници  " });

        var category = context.Categories.Single();
        Assert.Equal("Хладилници", category.Name);
    }

    [Fact]
    public async Task UpdateAsync_ChangesName()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.CreateAsync(new CategoryFormViewModel { Name = "Перални" });
        var category = context.Categories.Single();

        await service.UpdateAsync(new CategoryFormViewModel { Id = category.Id, Name = "Пералня" });

        Assert.Equal("Пералня", context.Categories.Single().Name);
    }

    [Fact]
    public async Task UpdateAsync_MissingCategory_Throws()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateAsync(new CategoryFormViewModel { Id = 999, Name = "X" }));
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithoutProducts_Deletes()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.CreateAsync(new CategoryFormViewModel { Name = "Печки" });
        var category = context.Categories.Single();

        await service.DeleteAsync(category.Id);

        Assert.Empty(context.Categories);
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithProducts_Throws()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.CreateAsync(new CategoryFormViewModel { Name = "Печки" });
        var category = context.Categories.Single();

        context.Products.Add(new TopDom.Models.Product
        {
            Name = "Печка X",
            Price = 100,
            Stock = 5,
            CategoryId = category.Id
        });
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(category.Id));
        Assert.Contains("продукти", ex.Message);
        Assert.Single(context.Categories);
    }
}