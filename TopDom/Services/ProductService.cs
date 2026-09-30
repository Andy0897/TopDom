using Microsoft.AspNetCore.Mvc.Rendering;
using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Products;

namespace TopDom.Services;

public class ProductService : IProductService
{
    private const int PageSize = 9;
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxImageSize = 2 * 1024 * 1024; // 2 MB

    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;

    public ProductService(IProductRepository products, ICategoryRepository categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<ProductListViewModel> GetListAsync(string? search, int? categoryId, int page)
    {
        if (page < 1) page = 1;

        var items = await _products.SearchAsync(search, categoryId, page, PageSize);
        var total = await _products.CountAsync(search, categoryId);
        var categories = await _categories.GetAllAsync();

        return new ProductListViewModel
        {
            Products = items.Select(p => new ProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name,
                HasImage = p.ImageContentType != null
            }).ToList(),
            Categories = categories,
            Search = search,
            CategoryId = categoryId,
            Page = page,
            TotalPages = (int)Math.Ceiling(total / (double)PageSize)
        };
    }

    public async Task<Product?> GetWithCategoryAsync(int id) => await _products.GetWithCategoryAsync(id);

    public async Task CreateAsync(ProductFormViewModel model)
    {
        var product = new Product
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Price = model.Price,
            Stock = model.Stock,
            CategoryId = model.CategoryId
        };

        await ApplyImageAsync(product, model.ImageFile);

        await _products.AddAsync(product);
        await _products.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductFormViewModel model)
    {
        var product = await _products.GetByIdAsync(model.Id)
            ?? throw new Exception("Продуктът не е намерен.");

        product.Name = model.Name.Trim();
        product.Description = model.Description?.Trim();
        product.Price = model.Price;
        product.Stock = model.Stock;
        product.CategoryId = model.CategoryId;

        await ApplyImageAsync(product, model.ImageFile);   // само ако е качена нова снимка

        _products.Update(product);
        await _products.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new Exception("Продуктът не е намерен.");

        _products.Delete(product);
        await _products.SaveChangesAsync();
    }

    private static async Task ApplyImageAsync(Product product, IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return; // при Edit без нова снимка старата се запазва

        if (!AllowedTypes.Contains(imageFile.ContentType))
            throw new Exception("Позволени са само снимки във формат JPG, PNG или WEBP.");

        if (imageFile.Length > MaxImageSize)
            throw new Exception("Снимката трябва да е до 2 MB.");

        using var ms = new MemoryStream();
        await imageFile.CopyToAsync(ms);
        product.ImageData = ms.ToArray();
        product.ImageContentType = imageFile.ContentType;
    }
}