using TopDom.Models;

namespace TopDom.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart> GetOrCreateAsync(int userId);
    Task<CartItem?> GetItemAsync(int cartId, int productId);
    Task AddItemAsync(CartItem item);
    void RemoveItem(CartItem item);
    void UpdateItem(CartItem item);
    Task SaveChangesAsync();
}