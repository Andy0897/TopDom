using System.ComponentModel.DataAnnotations;

namespace TopDom.ViewModels.Orders;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Въведете адрес.")]
    [StringLength(200)]
    [Display(Name = "Адрес за доставка")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Въведете телефон.")]
    [Phone(ErrorMessage = "Невалиден телефонен номер.")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;
}