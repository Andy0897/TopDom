using TopDom.ViewModels.Cart;

namespace TopDom.Services.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync(int userId);
    Task AddToCartAsync(int userId, int productId, int quantity);
    Task UpdateQuantityAsync(int userId, int productId, int quantity);
    Task RemoveFromCartAsync(int userId, int productId);
}