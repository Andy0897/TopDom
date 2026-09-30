using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services;
using TopDom.ViewModels.Products;

namespace TopDom.Tests;

public class ProductServiceTests
{
    private static async Task<(ProductService service, int categoryId)> SetupAsync(TopDom.Data.ApplicationDbContext context)
    {
        var category = new Category { Name = "Хладилници" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new ProductService(new ProductRepository(context), new CategoryRepository(context));
        return (service, category.Id);
    }

    [Fact]
    public async Task CreateAsync_WithoutImage_SavesProduct()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);

        await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Хладилник X",
            Price = 999.99m,
            Stock = 10,
            CategoryId = categoryId
        });

        var product = context.Products.Single();
        Assert.Equal("Хладилник X", product.Name);
        Assert.Null(product.ImageData);
    }

    [Fact]
    public async Task CreateAsync_WithValidImage_StoresImageBytes()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);

        await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Хладилник X",
            Price = 999.99m,
            Stock = 10,
            CategoryId = categoryId,
            ImageFile = TestFile.Create("image/jpeg", 1000)
        });

        var product = context.Products.Single();
        Assert.NotNull(product.ImageData);
        Assert.Equal("image/jpeg", product.ImageContentType);
    }

    [Fact]
    public async Task CreateAsync_ImageTooLarge_Throws()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);

        var model = new ProductFormViewModel
        {
            Name = "Хладилник X",
            Price = 999.99m,
            Stock = 10,
            CategoryId = categoryId,
            ImageFile = TestFile.Create("image/jpeg", 3 * 1024 * 1024)
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(model));
        Assert.Contains("2 MB", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_DisallowedContentType_Throws()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);

        var model = new ProductFormViewModel
        {
            Name = "Хладилник X",
            Price = 999.99m,
            Stock = 10,
            CategoryId = categoryId,
            ImageFile = TestFile.Create("application/pdf", 1000)
        };

        await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(model));
    }

    [Fact]
    public async Task UpdateAsync_WithoutNewImage_KeepsExistingImage()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);
        await service.CreateAsync(new ProductFormViewModel
        {
            Name = "Хладилник X",
            Price = 999.99m,
            Stock = 10,
            CategoryId = categoryId,
            ImageFile = TestFile.Create("image/jpeg", 1000)
        });
        var product = context.Products.Single();
        var originalImage = product.ImageData;

        await service.UpdateAsync(new ProductFormViewModel
        {
            Id = product.Id,
            Name = "Хладилник Y",
            Price = 899.99m,
            Stock = 5,
            CategoryId = categoryId
        });

        var updated = context.Products.Single();
        Assert.Equal("Хладилник Y", updated.Name);
        Assert.Equal(originalImage, updated.ImageData);
    }

    [Fact]
    public async Task GetListAsync_FiltersBySearchAndCategory()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);
        var otherCategory = new Category { Name = "Печки" };
        context.Categories.Add(otherCategory);
        await context.SaveChangesAsync();

        await service.CreateAsync(new ProductFormViewModel { Name = "Хладилник Samsung", Price = 1, Stock = 1, CategoryId = categoryId });
        await service.CreateAsync(new ProductFormViewModel { Name = "Хладилник LG", Price = 1, Stock = 1, CategoryId = categoryId });
        await service.CreateAsync(new ProductFormViewModel { Name = "Печка Bosch", Price = 1, Stock = 1, CategoryId = otherCategory.Id });

        var result = await service.GetListAsync("Хладилник", null, 1);

        Assert.Equal(2, result.Products.Count);
        Assert.All(result.Products, p => Assert.Contains("Хладилник", p.Name));
    }

    [Fact]
    public async Task GetListAsync_Pagination_ReturnsCorrectTotalPages()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);

        for (int i = 1; i <= 20; i++)
            await service.CreateAsync(new ProductFormViewModel { Name = $"Продукт {i}", Price = 1, Stock = 1, CategoryId = categoryId });

        var result = await service.GetListAsync(null, null, 1);

        Assert.Equal(9, result.Products.Count);   // PageSize = 9
        Assert.Equal(3, result.TotalPages);       // 20 / 9 закръглено нагоре
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        using var context = TestDb.Create();
        var (service, categoryId) = await SetupAsync(context);
        await service.CreateAsync(new ProductFormViewModel { Name = "X", Price = 1, Stock = 1, CategoryId = categoryId });
        var product = context.Products.Single();

        await service.DeleteAsync(product.Id);

        Assert.Empty(context.Products);
    }
}