using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TopDom.ViewModels.Products;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Въведете име.")]
    [StringLength(100)]
    [Display(Name = "Име")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Въведете цена.")]
    [Range(0.01, 1000000, ErrorMessage = "Цената трябва да е положително число.")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Въведете наличност.")]
    [Range(0, 100000, ErrorMessage = "Наличността не може да е отрицателна.")]
    [Display(Name = "Наличност")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Изберете категория.")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    [Display(Name = "Снимка")]
    public IFormFile? ImageFile { get; set; }

    public bool HasExistingImage { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}