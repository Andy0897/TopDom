namespace TopDom.Models;

public class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public List<CartItem> Items { get; set; } = new();
}