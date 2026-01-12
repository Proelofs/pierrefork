using Microsoft.EntityFrameworkCore;
using VerzekeringApi.Data;

namespace VerzekeringApi.Tests.TestHelpers;

public static class DbTestUtils
{
    public static AppDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }
}
