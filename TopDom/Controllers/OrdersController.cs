using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TopDom.Models;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Orders;

namespace TopDom.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ---------- Потребител ----------

    [Authorize(Roles = "User")]
    [HttpGet]
    public IActionResult Checkout() => View(new CheckoutViewModel());

    [Authorize(Roles = "User")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var orderId = await _orderService.CreateFromCartAsync(CurrentUserId, model);
            TempData["Success"] = "Поръчката е направена успешно.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var orders = await _orderService.GetMyOrdersAsync(CurrentUserId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var order = await _orderService.GetDetailsAsync(id, CurrentUserId, isAdmin);

        if (order == null) return NotFound();

        return View(order);
    }

    // ---------- Администратор ----------

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> AllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return View(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int orderId, OrderStatus status)
    {
        await _orderService.ChangeStatusAsync(orderId, status);
        return RedirectToAction(nameof(AllOrders));
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}