using TopDom.Models;

namespace TopDom.ViewModels.Products;

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool HasImage { get; set; }
}

public class ProductListViewModel
{
    public List<ProductListItemViewModel> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    public string? Search { get; set; }
    public int? CategoryId { get; set; }

    public int Page { get; set; }
    public int TotalPages { get; set; }
}