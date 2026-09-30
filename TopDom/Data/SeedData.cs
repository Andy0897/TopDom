using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TopDom.Models;
using TopDom.Services;
using TopDom.ViewModels.Account;

namespace TopDom.Data;

public static class SeedData
{
    private static readonly PasswordHasher<ApplicationUser> _hasher = new();

    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync(u => u.Role == "Admin"))
        {
            var admin = new ApplicationUser
            {
                FirstName = "Системен",
                LastName = "Администратор",
                Email = "admin@topdom.bg",
                Role = "Admin"
            };
            admin.PasswordHash = _hasher.HashPassword(admin, "Pass1234");
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}