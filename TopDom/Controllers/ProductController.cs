using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Products;

namespace TopDom.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    // ---------- Публична част ----------

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
    {
        var model = await _productService.GetListAsync(search, categoryId, page);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetWithCategoryAsync(id);
        if (product == null) return NotFound();

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Image(int id)
    {
        var product = await _productService.GetWithCategoryAsync(id);

        if (product?.ImageData == null)
            return NotFound();

        return File(product.ImageData, product.ImageContentType ?? "image/jpeg");
    }

    // ---------- Административна част ----------

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel { Categories = await GetCategorySelectListAsync() };
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategorySelectListAsync();
            return View(model);
        }

        try
        {
            await _productService.CreateAsync(model);
            TempData["Success"] = "Продуктът беше добавен.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.Categories = await GetCategorySelectListAsync();
            return View(model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetWithCategoryAsync(id);
        if (product == null) return NotFound();

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            HasExistingImage = product.ImageData != null,
            Categories = await GetCategorySelectListAsync()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategorySelectListAsync();
            return View(model);
        }

        try
        {
            await _productService.UpdateAsync(model);
            TempData["Success"] = "Продуктът беше обновен.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.Categories = await GetCategorySelectListAsync();
            return View(model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Продуктът беше изтрит.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategorySelectListAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        return categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();
    }
}