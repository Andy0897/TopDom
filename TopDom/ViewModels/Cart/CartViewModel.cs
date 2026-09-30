namespace TopDom.ViewModels.Cart;

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public bool HasImage { get; set; }

    public decimal Total => Price * Quantity;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal GrandTotal => Items.Sum(i => i.Total);
}