using TopDom.Models;

namespace TopDom.ViewModels.Orders;

public class OrderItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Price * Quantity;
}

public class OrderViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string? CustomerName { get; set; }   // само за admin списъка

    public List<OrderItemViewModel> Items { get; set; } = new();

    public decimal GrandTotal => Items.Sum(i => i.Total);
}