using Microsoft.EntityFrameworkCore;
using TopDom.Data;

namespace TopDom.Tests;

public static class TestDb
{
    // Всеки тест получава собствена празна база
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}