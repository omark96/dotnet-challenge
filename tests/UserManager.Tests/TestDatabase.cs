using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserManager.Web.Data;

namespace UserManager.Tests;

internal sealed class TestDatabase : IDbContextFactory<UserManagerDbContext>, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<UserManagerDbContext> _options;

    private TestDatabase(SqliteConnection connection)
    {
        _connection = connection;
        _options = new DbContextOptionsBuilder<UserManagerDbContext>()
            .UseSqlite(connection.ConnectionString)
            .Options;
    }

    public static async Task<TestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection(
            $"Data Source=UserManager-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=False");
        await connection.OpenAsync();
        var database = new TestDatabase(connection);
        await using var context = database.CreateDbContext();
        await context.Database.EnsureCreatedAsync();
        return database;
    }

    public UserManagerDbContext CreateDbContext() => new(_options);

    public Task<UserManagerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateDbContext());
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
