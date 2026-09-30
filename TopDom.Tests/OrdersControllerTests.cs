using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TopDom.Controllers;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Orders;

namespace TopDom.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderService = new();

    private OrdersController CreateController(int userId = 1, bool isAdmin = false)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (isAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        return new OrdersController(_orderService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Checkout_Post_Success_RedirectsToDetails()
    {
        _orderService.Setup(s => s.CreateFromCartAsync(1, It.IsAny<CheckoutViewModel>())).ReturnsAsync(42);
        var controller = CreateController();

        var result = await controller.Checkout(new CheckoutViewModel { Address = "ул. Х", Phone = "0888000000" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(OrdersController.Details), redirect.ActionName);
        Assert.Equal(42, redirect.RouteValues!["id"]);
    }

    [Fact]
    public async Task Checkout_Post_ServiceThrows_ReturnsViewWithError()
    {
        _orderService.Setup(s => s.CreateFromCartAsync(1, It.IsAny<CheckoutViewModel>()))
            .ThrowsAsync(new Exception("Количката е празна."));
        var controller = CreateController();

        var result = await controller.Checkout(new CheckoutViewModel { Address = "ул. Х", Phone = "0888000000" });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Details_NonAdmin_OtherUsersOrder_ReturnsNotFound()
    {
        _orderService.Setup(s => s.GetDetailsAsync(5, 1, false)).ReturnsAsync((OrderViewModel?)null);
        var controller = CreateController(userId: 1, isAdmin: false);

        var result = await controller.Details(5);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_Admin_PassesIsAdminTrue()
    {
        _orderService.Setup(s => s.GetDetailsAsync(5, 1, true)).ReturnsAsync(new OrderViewModel());
        var controller = CreateController(userId: 1, isAdmin: true);

        var result = await controller.Details(5);

        Assert.IsType<ViewResult>(result);
        _orderService.Verify(s => s.GetDetailsAsync(5, 1, true), Times.Once);
    }

    [Fact]
    public async Task AllOrders_ReturnsViewWithAllOrders()
    {
        _orderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(new List<OrderViewModel>());
        var controller = CreateController(isAdmin: true);

        var result = await controller.AllOrders();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task ChangeStatus_CallsServiceAndRedirects()
    {
        var controller = CreateController(isAdmin: true);

        var result = await controller.ChangeStatus(7, TopDom.Models.OrderStatus.Изпратена);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(OrdersController.AllOrders), redirect.ActionName);
        _orderService.Verify(s => s.ChangeStatusAsync(7, TopDom.Models.OrderStatus.Изпратена), Times.Once);
    }
}