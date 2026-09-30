using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services;
using TopDom.ViewModels.Orders;

namespace TopDom.Tests;

public class OrderServiceTests
{
    private static async Task<(OrderService service, CartService cartService, int userId, int productId)> SetupAsync(
        TopDom.Data.ApplicationDbContext context, int stock = 10)
    {
        var user = new ApplicationUser { FirstName = "И", LastName = "П", Email = "a@a.bg", PasswordHash = "h" };
        var category = new Category { Name = "Категория" };
        context.Users.Add(user);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new Product { Name = "Продукт", Price = 50, Stock = stock, CategoryId = category.Id };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var productRepo = new ProductRepository(context);
        var cartRepo = new CartRepository(context);

        var cartService = new CartService(cartRepo, productRepo);
        var orderService = new OrderService(new OrderRepository(context), cartRepo, productRepo);

        return (orderService, cartService, user.Id, product.Id);
    }

    private static CheckoutViewModel ValidCheckout() => new() { Address = "ул. Тест 1", Phone = "0888123456" };

    [Fact]
    public async Task CreateFromCartAsync_EmptyCart_Throws()
    {
        using var context = TestDb.Create();
        var (orderService, _, userId, _) = await SetupAsync(context);

        await Assert.ThrowsAsync<Exception>(() => orderService.CreateFromCartAsync(userId, ValidCheckout()));
    }

    [Fact]
    public async Task CreateFromCartAsync_ValidCart_CreatesOrderAndClearsCart()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context);
        await cartService.AddToCartAsync(userId, productId, 2);

        var orderId = await orderService.CreateFromCartAsync(userId, ValidCheckout());

        Assert.True(orderId > 0);
        var cart = await cartService.GetCartAsync(userId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task CreateFromCartAsync_ReducesStock()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context, stock: 10);
        await cartService.AddToCartAsync(userId, productId, 3);

        await orderService.CreateFromCartAsync(userId, ValidCheckout());

        var product = context.Products.Single();
        Assert.Equal(7, product.Stock);
    }

    [Fact]
    public async Task CreateFromCartAsync_PriceAtOrderTime()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context);
        await cartService.AddToCartAsync(userId, productId, 1);

        // цената се променя СЛЕД добавяне в количката, но преди поръчката
        var product = context.Products.Single();
        product.Price = 999;
        await context.SaveChangesAsync();

        var orderId = await orderService.CreateFromCartAsync(userId, ValidCheckout());

        var order = await orderService.GetDetailsAsync(orderId, userId, isAdmin: false);
        Assert.Equal(999, order!.Items[0].Price);   // старата цена, не новата
    }

    [Fact]
    public async Task GetMyOrdersAsync_ReturnsOnlyOwnOrders()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context);
        await cartService.AddToCartAsync(userId, productId, 1);
        await orderService.CreateFromCartAsync(userId, ValidCheckout());

        var otherUser = new ApplicationUser { FirstName = "Д", LastName = "Р", Email = "b@b.bg", PasswordHash = "h" };
        context.Users.Add(otherUser);
        await context.SaveChangesAsync();

        var myOrders = await orderService.GetMyOrdersAsync(userId);
        var otherOrders = await orderService.GetMyOrdersAsync(otherUser.Id);

        Assert.Single(myOrders);
        Assert.Empty(otherOrders);
    }

    [Fact]
    public async Task GetDetailsAsync_OtherUsersOrder_NonAdmin_ReturnsNull()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context);
        await cartService.AddToCartAsync(userId, productId, 1);
        var orderId = await orderService.CreateFromCartAsync(userId, ValidCheckout());

        var result = await orderService.GetDetailsAsync(orderId, userId: 999, isAdmin: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDetailsAsync_Admin_CanSeeAnyOrder()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context);
        await cartService.AddToCartAsync(userId, productId, 1);
        var orderId = await orderService.CreateFromCartAsync(userId, ValidCheckout());

        var result = await orderService.GetDetailsAsync(orderId, userId: 999, isAdmin: true);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ChangeStatusAsync_UpdatesStatus()
    {
        using var context = TestDb.Create();
        var (orderService, cartService, userId, productId) = await SetupAsync(context);
        await cartService.AddToCartAsync(userId, productId, 1);
        var orderId = await orderService.CreateFromCartAsync(userId, ValidCheckout());

        await orderService.ChangeStatusAsync(orderId, OrderStatus.Изпратена);

        var order = await orderService.GetDetailsAsync(orderId, userId, isAdmin: true);
        Assert.Equal(OrderStatus.Изпратена, order!.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_MissingOrder_Throws()
    {
        using var context = TestDb.Create();
        var (orderService, _, _, _) = await SetupAsync(context);

        await Assert.ThrowsAsync<Exception>(() => orderService.ChangeStatusAsync(999, OrderStatus.Доставена));
    }
}