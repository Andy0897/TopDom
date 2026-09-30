using TopDom.Repositories;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Cart;

namespace TopDom.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;

    public CartService(ICartRepository carts, IProductRepository products)
    {
        _carts = carts;
        _products = products;
    }

    public async Task<CartViewModel> GetCartAsync(int userId)
    {
        var cart = await _carts.GetByUserIdAsync(userId);

        if (cart == null)
            return new CartViewModel();

        return new CartViewModel
        {
            Items = cart.Items.Select(i => new CartItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                Quantity = i.Quantity,
                Stock = i.Product.Stock,
                HasImage = i.Product.ImageContentType != null
            }).ToList()
        };
    }

    public async Task AddToCartAsync(int userId, int productId, int quantity)
    {
        if (quantity < 1)
            throw new Exception("Количеството трябва да е поне 1.");

        var product = await _products.GetByIdAsync(productId)
            ?? throw new Exception("Продуктът не е намерен.");

        var cart = await _carts.GetOrCreateAsync(userId);
        var existingItem = await _carts.GetItemAsync(cart.Id, productId);

        var newQuantity = (existingItem?.Quantity ?? 0) + quantity;

        if (newQuantity > product.Stock)
            throw new Exception($"Наличността е само {product.Stock} бр.");

        if (existingItem != null)
        {
            existingItem.Quantity = newQuantity;
            _carts.UpdateItem(existingItem);
        }
        else
        {
            await _carts.AddItemAsync(new()
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            });
        }

        await _carts.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        if (quantity < 1)
            throw new Exception("Количеството трябва да е поне 1.");

        var cart = await _carts.GetOrCreateAsync(userId);
        var item = await _carts.GetItemAsync(cart.Id, productId)
            ?? throw new Exception("Продуктът не е в количката.");

        var product = await _products.GetByIdAsync(productId)
            ?? throw new Exception("Продуктът не е намерен.");

        if (quantity > product.Stock)
            throw new Exception($"Наличността е само {product.Stock} бр.");

        item.Quantity = quantity;
        _carts.UpdateItem(item);
        await _carts.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(int userId, int productId)
    {
        var cart = await _carts.GetOrCreateAsync(userId);
        var item = await _carts.GetItemAsync(cart.Id, productId);

        if (item != null)
        {
            _carts.RemoveItem(item);
            await _carts.SaveChangesAsync();
        }
    }
}