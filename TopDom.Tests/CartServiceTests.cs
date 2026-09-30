using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services;

namespace TopDom.Tests;

public class CartServiceTests
{
    private static async Task<(CartService service, int userId, int productId)> SetupAsync(
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

        var service = new CartService(new CartRepository(context), new ProductRepository(context));
        return (service, user.Id, product.Id);
    }

    [Fact]
    public async Task AddToCartAsync_NewProduct_AddsItem()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);

        await service.AddToCartAsync(userId, productId, 2);

        var cart = await service.GetCartAsync(userId);
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_ExistingProduct_IncreasesQuantity()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);
        await service.AddToCartAsync(userId, productId, 2);

        await service.AddToCartAsync(userId, productId, 3);

        var cart = await service.GetCartAsync(userId);
        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items[0].Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_MoreThanStock_Throws()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context, stock: 3);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddToCartAsync(userId, productId, 5));
        Assert.Contains("3", ex.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ZeroOrNegativeQuantity_Throws()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);

        await Assert.ThrowsAsync<Exception>(() => service.AddToCartAsync(userId, productId, 0));
    }

    [Fact]
    public async Task UpdateQuantityAsync_ValidQuantity_Updates()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);
        await service.AddToCartAsync(userId, productId, 1);

        await service.UpdateQuantityAsync(userId, productId, 4);

        var cart = await service.GetCartAsync(userId);
        Assert.Equal(4, cart.Items[0].Quantity);
    }

    [Fact]
    public async Task UpdateQuantityAsync_ProductNotInCart_Throws()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);

        await Assert.ThrowsAsync<Exception>(() => service.UpdateQuantityAsync(userId, productId, 2));
    }

    [Fact]
    public async Task RemoveFromCartAsync_RemovesItem()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);
        await service.AddToCartAsync(userId, productId, 1);

        await service.RemoveFromCartAsync(userId, productId);

        var cart = await service.GetCartAsync(userId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task GetCartAsync_NoCartYet_ReturnsEmptyViewModel()
    {
        using var context = TestDb.Create();
        var (service, userId, _) = await SetupAsync(context);

        var cart = await service.GetCartAsync(userId);

        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.GrandTotal);
    }

    [Fact]
    public async Task GetCartAsync_GrandTotal_IsSumOfItemTotals()
    {
        using var context = TestDb.Create();
        var (service, userId, productId) = await SetupAsync(context);
        await service.AddToCartAsync(userId, productId, 3);   // 3 x 50 = 150

        var cart = await service.GetCartAsync(userId);

        Assert.Equal(150, cart.GrandTotal);
    }
}