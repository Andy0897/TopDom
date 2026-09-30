using System.ComponentModel.DataAnnotations;

namespace TopDom.ViewModels.Categories;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Въведете име.")]
    [StringLength(50)]
    [Display(Name = "Име")]
    public string Name { get; set; } = string.Empty;
}