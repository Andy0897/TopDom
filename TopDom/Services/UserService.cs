using TopDom.Models;
using TopDom.Repositories;
using TopDom.Services.Interfaces;
using TopDom.ViewModels.Account;

namespace TopDom.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly PasswordService _passwordService = new();

    public UserService(IUserRepository users, PasswordService passwordService)
    {
        _users = users;
        _passwordService = passwordService;
    }

    public async Task RegisterAsync(RegisterViewModel model, string role = "User")
    {
        var email = model.Email.Trim().ToLower();

        if (await _users.EmailExistsAsync(email))
            throw new Exception("Вече има потребител с този имейл.");

        var user = new ApplicationUser
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordService.Hash(model.Password),
            Role = role
        };

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
    }

    public async Task<ApplicationUser?> LoginAsync(LoginViewModel model)
    {
        var user = await _users.GetByEmailAsync(model.Email.Trim().ToLower());

        if (user == null || !_passwordService.Verify(user.PasswordHash, model.Password))
            return null;

        return user;
    }
}