using TopDom.Models;
using TopDom.Repositories;

namespace TopDom.Tests;

public class UserRepositoryTests
{
    private static ApplicationUser NewUser(string email, string role = "User") => new()
    {
        FirstName = "Тест",
        LastName = "Потребител",
        Email = email,
        PasswordHash = "hash",
        Role = role
    };

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        using var context = TestDb.Create();
        var repo = new UserRepository(context);
        await repo.AddAsync(NewUser("a@test.bg"));
        await repo.SaveChangesAsync();

        var user = await repo.GetByEmailAsync("a@test.bg");

        Assert.NotNull(user);
        Assert.Equal("a@test.bg", user!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_MissingEmail_ReturnsNull()
    {
        using var context = TestDb.Create();
        var repo = new UserRepository(context);

        var user = await repo.GetByEmailAsync("missing@test.bg");

        Assert.Null(user);
    }

    [Fact]
    public async Task EmailExistsAsync_ReturnsTrueOnlyForExistingEmail()
    {
        using var context = TestDb.Create();
        var repo = new UserRepository(context);
        await repo.AddAsync(NewUser("a@test.bg"));
        await repo.SaveChangesAsync();

        Assert.True(await repo.EmailExistsAsync("a@test.bg"));
        Assert.False(await repo.EmailExistsAsync("b@test.bg"));
    }

    [Fact]
    public async Task AnyAdminAsync_NoAdmins_ReturnsFalse()
    {
        using var context = TestDb.Create();
        var repo = new UserRepository(context);
        await repo.AddAsync(NewUser("a@test.bg", "User"));
        await repo.SaveChangesAsync();

        Assert.False(await repo.AnyAdminAsync());
    }

    [Fact]
    public async Task AnyAdminAsync_WithAdmin_ReturnsTrue()
    {
        using var context = TestDb.Create();
        var repo = new UserRepository(context);
        await repo.AddAsync(NewUser("admin@test.bg", "Admin"));
        await repo.SaveChangesAsync();

        Assert.True(await repo.AnyAdminAsync());
    }

    [Fact]
    public async Task GetByIdAsync_AndGetAllAsync_WorkThroughGenericRepository()
    {
        using var context = TestDb.Create();
        var repo = new UserRepository(context);
        await repo.AddAsync(NewUser("a@test.bg"));
        await repo.AddAsync(NewUser("b@test.bg"));
        await repo.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        var first = await repo.GetByIdAsync(all[0].Id);

        Assert.Equal(2, all.Count);
        Assert.NotNull(first);
    }
}