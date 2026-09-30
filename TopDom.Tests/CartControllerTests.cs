using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TopDom.Controllers;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Cart;

namespace TopDom.Tests;

public class CartControllerTests
{
    private readonly Mock<ICartService> _cartService = new();

    private CartController CreateController(int userId = 1)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth");

        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        return new CartController(_cartService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithCart()
    {
        _cartService.Setup(s => s.GetCartAsync(1)).ReturnsAsync(new CartViewModel());
        var controller = CreateController();

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Add_Success_RedirectsToProductDetails()
    {
        var controller = CreateController();

        var result = await controller.Add(productId: 5, quantity: 2);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Products", redirect.ControllerName);
        _cartService.Verify(s => s.AddToCartAsync(1, 5, 2), Times.Once);
    }

    [Fact]
    public async Task Add_ServiceThrows_SetsErrorTempDataAndRedirects()
    {
        _cartService.Setup(s => s.AddToCartAsync(1, 5, 2)).ThrowsAsync(new Exception("Няма наличност."));
        var controller = CreateController();

        var result = await controller.Add(productId: 5, quantity: 2);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Няма наличност.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task Remove_CallsServiceAndRedirectsToIndex()
    {
        var controller = CreateController();

        var result = await controller.Remove(productId: 5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(CartController.Index), redirect.ActionName);
        _cartService.Verify(s => s.RemoveFromCartAsync(1, 5), Times.Once);
    }
}