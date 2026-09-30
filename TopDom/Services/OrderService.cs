using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Orders;

namespace TopDom.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;

    public OrderService(IOrderRepository orders, ICartRepository carts, IProductRepository products)
    {
        _orders = orders;
        _carts = carts;
        _products = products;
    }

    public async Task<int> CreateFromCartAsync(int userId, CheckoutViewModel model)
    {
        var cart = await _carts.GetByUserIdAsync(userId);

        if (cart == null || !cart.Items.Any())
            throw new Exception("Количката е празна.");

        // проверка на наличността за всеки продукт, преди да пипнем нещо
        foreach (var item in cart.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId)
                ?? throw new Exception("Продукт от количката вече не съществува.");

            if (item.Quantity > product.Stock)
                throw new Exception($"'{product.Name}' няма достатъчна наличност ({product.Stock} бр.).");
        }

        var order = new Order
        {
            UserId = userId,
            Address = model.Address.Trim(),
            Phone = model.Phone.Trim(),
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Product.Price   // цената се "замразява" към момента на поръчката
            }).ToList()
        };

        await _orders.CreateAsync(order);

        // намаляване на наличността
        foreach (var item in cart.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId);
            product!.Stock -= item.Quantity;
            _products.Update(product);
        }
        await _products.SaveChangesAsync();

        // изчистване на количката
        foreach (var item in cart.Items.ToList())
            _carts.RemoveItem(item);
        await _carts.SaveChangesAsync();

        return order.Id;
    }

    public async Task<List<OrderViewModel>> GetMyOrdersAsync(int userId)
    {
        var orders = await _orders.GetByUserIdAsync(userId);
        return orders.Select(MapToViewModel).ToList();
    }

    public async Task<List<OrderViewModel>> GetAllOrdersAsync()
    {
        var orders = await _orders.GetAllAsync();
        return orders.Select(o =>
        {
            var vm = MapToViewModel(o);
            vm.CustomerName = $"{o.User.FirstName} {o.User.LastName}";
            return vm;
        }).ToList();
    }

    public async Task<OrderViewModel?> GetDetailsAsync(int id, int userId, bool isAdmin)
    {
        var order = isAdmin
            ? await _orders.GetByIdAsync(id)
            : await _orders.GetByIdForUserAsync(id, userId);

        return order == null ? null : MapToViewModel(order);
    }

    public async Task ChangeStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _orders.GetByIdAsync(orderId)
            ?? throw new Exception("Поръчката не е намерена.");

        _orders.UpdateStatus(order, status);
        await _orders.SaveChangesAsync();
    }

    private static OrderViewModel MapToViewModel(Order o) => new()
    {
        Id = o.Id,
        OrderDate = o.OrderDate,
        Status = o.Status,
        Address = o.Address,
        Phone = o.Phone,
        Items = o.Items.Select(i => new OrderItemViewModel
        {
            ProductName = i.Product.Name,
            Price = i.Price,
            Quantity = i.Quantity
        }).ToList()
    };
}