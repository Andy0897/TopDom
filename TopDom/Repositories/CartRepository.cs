using Microsoft.EntityFrameworkCore;
using TopDom.Data;
using TopDom.Models;

namespace TopDom.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId) =>
        await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart> GetOrCreateAsync(int userId)
    {
        var cart = await GetByUserIdAsync(userId);
        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<CartItem?> GetItemAsync(int cartId, int productId) =>
        await _context.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);

    public async Task AddItemAsync(CartItem item) =>
        await _context.CartItems.AddAsync(item);

    public void RemoveItem(CartItem item) => _context.CartItems.Remove(item);

    public void UpdateItem(CartItem item) => _context.CartItems.Update(item);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}