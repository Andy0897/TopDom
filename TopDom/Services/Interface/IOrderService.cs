using TopDom.Models;
using TopDom.ViewModels.Orders;

namespace TopDom.Services.Interfaces;

public interface IOrderService
{
    Task<int> CreateFromCartAsync(int userId, CheckoutViewModel model);
    Task<List<OrderViewModel>> GetMyOrdersAsync(int userId);
    Task<List<OrderViewModel>> GetAllOrdersAsync();
    Task<OrderViewModel?> GetDetailsAsync(int id, int userId, bool isAdmin);
    Task ChangeStatusAsync(int orderId, OrderStatus status);
}