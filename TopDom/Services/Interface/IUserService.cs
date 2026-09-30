using TopDom.Models;
using TopDom.ViewModels.Account;

namespace TopDom.Services.Interfaces;

public interface IUserService
{
    Task RegisterAsync(RegisterViewModel model, string role = "User");
    Task<ApplicationUser?> LoginAsync(LoginViewModel model);
}