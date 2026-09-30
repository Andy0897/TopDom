using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TopDom.Repositories;
using TopDom.Services;
using TopDom.ViewModels.Account;

namespace TopDom.Tests;

public class UserServiceTests
{
    private readonly PasswordService _passwordService;

    public UserServiceTests()
    {
        // Ensure the password hasher is initialized before tests
        _passwordService = new PasswordService();
    }

    private static RegisterViewModel ValidRegisterModel(string email = "ivan@test.bg") => new()
    {
        FirstName = "Иван",
        LastName = "Петров",
        Email = email,
        Password = "Secret123",
        ConfirmPassword = "Secret123"
    };

    private static UserService CreateService(TopDom.Data.ApplicationDbContext context) =>
        new(new UserRepository(context), new PasswordService());


    [Fact]
    public async Task RegisterAsync_ValidModel_SavesUser()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await service.RegisterAsync(ValidRegisterModel());

        var user = await context.Users.SingleAsync();
        Assert.Equal("Иван", user.FirstName);
        Assert.Equal("Петров", user.LastName);
        Assert.Equal("ivan@test.bg", user.Email);
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed_NotStoredAsPlainText()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await service.RegisterAsync(ValidRegisterModel());

        var user = await context.Users.SingleAsync();
        Assert.NotEqual("Secret123", user.PasswordHash);
        Assert.True(_passwordService.Verify(user.PasswordHash, "Secret123"));
    }

    [Fact]
    public async Task RegisterAsync_DefaultRole_IsUser()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await service.RegisterAsync(ValidRegisterModel());

        var user = await context.Users.SingleAsync();
        Assert.Equal("User", user.Role);
    }

    [Fact]
    public async Task RegisterAsync_WithAdminRole_SavesAdminRole()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await service.RegisterAsync(ValidRegisterModel("admin@test.bg"), "Admin");

        var user = await context.Users.SingleAsync();
        Assert.Equal("Admin", user.Role);
    }

    [Fact]
    public async Task RegisterAsync_EmailIsTrimmedAndLowercased()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        await service.RegisterAsync(ValidRegisterModel("  IVAN@Test.BG  "));

        var user = await context.Users.SingleAsync();
        Assert.Equal("ivan@test.bg", user.Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsException()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegisterModel());

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.RegisterAsync(ValidRegisterModel()));

        Assert.Equal("Вече има потребител с този имейл.", ex.Message);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmailWithDifferentCase_ThrowsException()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegisterModel("ivan@test.bg"));

        await Assert.ThrowsAsync<Exception>(() =>
            service.RegisterAsync(ValidRegisterModel("IVAN@TEST.BG")));
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsUser()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegisterModel());

        var user = await service.LoginAsync(new LoginViewModel
        {
            Email = "ivan@test.bg",
            Password = "Secret123"
        });

        Assert.NotNull(user);
        Assert.Equal("ivan@test.bg", user!.Email);
    }

    [Fact]
    public async Task LoginAsync_EmailIsCaseInsensitive()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegisterModel());

        var user = await service.LoginAsync(new LoginViewModel
        {
            Email = " IVAN@Test.bg ",
            Password = "Secret123"
        });

        Assert.NotNull(user);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);
        await service.RegisterAsync(ValidRegisterModel());

        var user = await service.LoginAsync(new LoginViewModel
        {
            Email = "ivan@test.bg",
            Password = "WrongPassword"
        });

        Assert.Null(user);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsNull()
    {
        using var context = TestDb.Create();
        var service = CreateService(context);

        var user = await service.LoginAsync(new LoginViewModel
        {
            Email = "nobody@test.bg",
            Password = "Secret123"
        });

        Assert.Null(user);
    }
}