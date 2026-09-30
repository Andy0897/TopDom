using TopDom.Models;

namespace TopDom.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<List<Order>> GetByUserIdAsync(int userId);
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdForUserAsync(int id, int userId);
    void UpdateStatus(Order order, OrderStatus status);
    Task SaveChangesAsync();
}