using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using UserManager.Web.Models;
using UserManager.Web.Services;

namespace UserManager.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_PersistsUserAndSetsTimestamps()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);

        var created = await service.CreateAsync(ValidModel());

        await using var verification = database.CreateDbContext();
        var saved = await verification.Users.SingleAsync();
        Assert.Equal(created.Id, saved.Id);
        Assert.Equal("Ada", saved.FirstName);
        Assert.Equal("Lovelace", saved.LastName);
        Assert.Equal("ada.lovelace@example.com", saved.Email);
        Assert.True(saved.IsActive);
        Assert.NotEqual(default, saved.CreatedAt);
        Assert.Equal(saved.CreatedAt, saved.LastModifiedAt);
    }

    [Fact]
    public async Task CreateAndUpdateAsync_TrimContactDetails()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);
        var created = await service.CreateAsync(new UserFormModel
        {
            FirstName = "  Ada ",
            LastName = " Lovelace  ",
            Email = " ada.lovelace@example.com "
        });

        await using (var verification = database.CreateDbContext())
        {
            var saved = await verification.Users.SingleAsync();
            Assert.Equal("Ada", saved.FirstName);
            Assert.Equal("Lovelace", saved.LastName);
            Assert.Equal("ada.lovelace@example.com", saved.Email);
        }

        await service.UpdateAsync(created.Id, new UserFormModel
        {
            FirstName = "  Augusta ",
            LastName = " King ",
            Email = " ada.lovelace@example.com ",
            IsActive = false
        });

        await using var updatedVerification = database.CreateDbContext();
        var updated = await updatedVerification.Users.SingleAsync();
        Assert.Equal("Augusta", updated.FirstName);
        Assert.Equal("King", updated.LastName);
        Assert.False(updated.IsActive);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);
        Assert.True(updated.LastModifiedAt >= created.LastModifiedAt);
    }

    [Theory]
    [InlineData("Ada")]
    [InlineData("Lovelace")]
    [InlineData("  Ada  ")]
    [InlineData("\tLovelace ")]
    public async Task GetUsersAsync_FindsNameMatchesAndTrimsSearch(string term)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);
        var created = await service.CreateAsync(ValidModel());

        var match = Assert.Single(await service.GetUsersAsync(term));

        Assert.Equal(created.Id, match.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public async Task GetUsersAsync_EmptySearchReturnsAllUsers(string? term)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);
        await service.CreateAsync(ValidModel());
        await service.CreateAsync(new UserFormModel
        {
            FirstName = "Sam",
            LastName = "Rivera",
            Email = "sam.rivera@example.com",
            IsActive = false
        });

        Assert.Equal(2, (await service.GetUsersAsync(term)).Count);
    }

    [Fact]
    public async Task GetUsersAsync_NoMatchReturnsEmptyList()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);
        await service.CreateAsync(ValidModel());

        Assert.Empty(await service.GetUsersAsync("Nobody"));
    }

    [Fact]
    public async Task GetByIdAsync_MissingUserReturnsNull()
    {
        await using var database = await TestDatabase.CreateAsync();

        Assert.Null(await new UserService(database).GetByIdAsync(123));
    }

    [Fact]
    public async Task UpdateAsync_MissingUserReportsAnError()
    {
        await using var database = await TestDatabase.CreateAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new UserService(database).UpdateAsync(123, ValidModel()));

        Assert.Equal("The selected user could not be found.", exception.Message);
        await using var verification = database.CreateDbContext();
        Assert.Empty(await verification.Users.ToListAsync());
    }

    [Fact]
    public async Task SetActiveAsync_MissingUserReportsAnError()
    {
        await using var database = await TestDatabase.CreateAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new UserService(database).SetActiveAsync(123, false));

        Assert.Equal("The selected user could not be found.", exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetActiveAsync_PersistsStatusAndLastModifiedDate(bool isActive)
    {
        await using var database = await TestDatabase.CreateAsync();
        var lastModified = DateTimeOffset.UtcNow.AddDays(-1);
        var user = new User
        {
            FirstName = "Sam",
            LastName = "Rivera",
            Email = "sam.rivera@example.com",
            IsActive = !isActive,
            CreatedAt = lastModified.AddDays(-10),
            LastModifiedAt = lastModified
        };
        await using (var setup = database.CreateDbContext())
        {
            setup.Users.Add(user);
            await setup.SaveChangesAsync();
        }

        await new UserService(database).SetActiveAsync(user.Id, isActive);

        await using var verification = database.CreateDbContext();
        var saved = await verification.Users.SingleAsync();
        Assert.Equal(isActive, saved.IsActive);
        Assert.Equal(user.CreatedAt, saved.CreatedAt);
        Assert.True(saved.LastModifiedAt > lastModified);
    }

    [Fact]
    public async Task ImportCsvAsync_ImportsValidRowsAndReportsEachInvalidRow()
    {
        await using var database = await TestDatabase.CreateAsync();
        const string csv = """
            FirstName,LastName,Email,IsActive
            Leila,Demir,leila.demir@example.com,true
            Noah,Invalid,invalid@@example.com,false
            ,Missing,missing@example.com,true
            Alex,Smith,alex.smith@example.com,yes
            Too,Few,columns
            Mikael,Sundstrom,mikael.sundstrom@example.com,false
            """;

        var result = await new UserService(database).ImportCsvAsync(csv);

        Assert.Equal(2, result.ImportedCount);
        Assert.Equal(4, result.Errors.Count);
        Assert.Collection(result.Errors,
            error => Assert.StartsWith("Row 3:", error),
            error => Assert.StartsWith("Row 4:", error),
            error => Assert.StartsWith("Row 5:", error),
            error => Assert.StartsWith("Row 6:", error));
        await using var verification = database.CreateDbContext();
        var users = await verification.Users.OrderBy(user => user.FirstName).ToListAsync();
        Assert.Collection(users,
            user => { Assert.Equal("Leila", user.FirstName); Assert.True(user.IsActive); },
            user => { Assert.Equal("Mikael", user.FirstName); Assert.False(user.IsActive); });
    }

    [Fact]
    public async Task ImportCsvAsync_TrimsValuesAndAcceptsReorderedCaseInsensitiveHeaders()
    {
        await using var database = await TestDatabase.CreateAsync();
        const string csv = """
             email , ISACTIVE , lastname , firstname
             ada.lovelace@example.com , true , Lovelace , Ada
            """;

        var result = await new UserService(database).ImportCsvAsync(csv);

        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);
        await using var verification = database.CreateDbContext();
        var saved = await verification.Users.SingleAsync();
        Assert.Equal("Ada", saved.FirstName);
        Assert.Equal("Lovelace", saved.LastName);
        Assert.Equal("ada.lovelace@example.com", saved.Email);
    }

    [Fact]
    public async Task ImportCsvAsync_ReadsQuotedCommasEscapedQuotesAndLineBreaks()
    {
        await using var database = await TestDatabase.CreateAsync();
        const string csv = "FirstName,LastName,Email,IsActive\r\n"
            + "\"Ada, \"\"Countess\"\"\",\"Love\r\nlace\",ada@example.com,true";

        var result = await new UserService(database).ImportCsvAsync(csv);

        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);
        await using var verification = database.CreateDbContext();
        var saved = await verification.Users.SingleAsync();
        Assert.Equal("Ada, \"Countess\"", saved.FirstName);
        Assert.Equal("Love\r\nlace", saved.LastName);
    }

    [Fact]
    public async Task ImportCsvAsync_IgnoresBlankRows()
    {
        await using var database = await TestDatabase.CreateAsync();
        const string csv = "FirstName,LastName,Email,IsActive\n\n,,,\n"
            + "Ada,Lovelace,ada@example.com,true\n   \n";

        var result = await new UserService(database).ImportCsvAsync(csv);

        Assert.Equal(1, result.ImportedCount);
        Assert.Empty(result.Errors);
        await using var verification = database.CreateDbContext();
        Assert.Single(await verification.Users.ToListAsync());
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData(" \t\r\n", "empty")]
    [InlineData("FirstName,LastName,IsActive\nAda,Lovelace,true", "Email")]
    [InlineData("FirstName,LastName,Email,IsActive, email", "more than once")]
    [InlineData("Name,Address,Status", "FirstName")]
    public async Task ImportCsvAsync_InvalidHeadersDoNotWriteUsers(string csv, string message)
    {
        await using var database = await TestDatabase.CreateAsync();

        var exception = await Assert.ThrowsAsync<FormatException>(
            () => new UserService(database).ImportCsvAsync(csv));

        Assert.Contains(message, exception.Message);
        await using var verification = database.CreateDbContext();
        Assert.Empty(await verification.Users.ToListAsync());
    }

    [Theory]
    [InlineData("\"Ada,Lovelace,ada@example.com,true")]
    [InlineData("\"Ada\"unexpected,Lovelace,ada@example.com,true")]
    [InlineData("A\"da,Lovelace,ada@example.com,true")]
    public async Task ImportCsvAsync_MalformedQuotedFieldsDoNotWriteUsers(string row)
    {
        await using var database = await TestDatabase.CreateAsync();
        var csv = "FirstName,LastName,Email,IsActive\n"
            + "Sam,Rivera,sam@example.com,true\n" + row;

        await Assert.ThrowsAsync<FormatException>(() => new UserService(database).ImportCsvAsync(csv));

        await using var verification = database.CreateDbContext();
        Assert.Empty(await verification.Users.ToListAsync());
    }

    [Theory]
    [InlineData("FirstName,LastName,Email,IsActive", 0)]
    [InlineData("FirstName,LastName,Email,IsActive\nAda,Lovelace,not-an-email,true", 1)]
    public async Task ImportCsvAsync_NoValidRowsReturnsSummaryWithoutWriting(string csv, int errors)
    {
        await using var database = await TestDatabase.CreateAsync();

        var result = await new UserService(database).ImportCsvAsync(csv);

        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(errors, result.Errors.Count);
        await using var verification = database.CreateDbContext();
        Assert.Empty(await verification.Users.ToListAsync());
    }

    public static TheoryData<string, string, string, string> InvalidInputs => new()
    {
        { " ", "Lovelace", "ada@example.com", "First name is required." },
        { "Ada", "\t", "ada@example.com", "Last name is required." },
        { "Ada", "Lovelace", " ", "Email is required." },
        { "Ada", "Lovelace", "invalid@@example.com", "Enter a valid email address." },
        { new string('A', 81), "Lovelace", "ada@example.com", "First name cannot be longer than 80 characters." },
        { "Ada", new string('L', 81), "ada@example.com", "Last name cannot be longer than 80 characters." },
        { "Ada", "Lovelace", new string('a', 243) + "@example.com", "Email cannot be longer than 254 characters." },
        { "Ada", "Lovelace", "ada\n@example.com", "Enter a valid email address." }
    };

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public async Task WriteMethods_UseTheSameValidationAsTheFormAndCsv(
        string firstName, string lastName, string email, string message)
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);
        var model = new UserFormModel { FirstName = firstName, LastName = lastName, Email = email };
        Assert.Contains(model.Validate(), error => error.ErrorMessage == message);

        var createError = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(model));
        Assert.Contains(message, createError.Message);

        var original = await service.CreateAsync(ValidModel());
        var updateError = await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(original.Id, model));
        Assert.Contains(message, updateError.Message);

        var csv = "FirstName,LastName,Email,IsActive\n"
            + $"{Quote(firstName)},{Quote(lastName)},{Quote(email)},true";
        var import = await service.ImportCsvAsync(csv);
        Assert.Equal(0, import.ImportedCount);
        Assert.Contains(message, Assert.Single(import.Errors));

        await using var verification = database.CreateDbContext();
        var saved = await verification.Users.SingleAsync();
        Assert.Equal(original.FirstName, saved.FirstName);
        Assert.Equal(original.LastName, saved.LastName);
        Assert.Equal(original.Email, saved.Email);
        Assert.Equal(original.LastModifiedAt, saved.LastModifiedAt);
    }

    [Fact]
    public async Task GetUsersAsync_ConcurrentReadsAndLaterWritesUseIndependentContexts()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = new UserService(database);
        var original = await service.CreateAsync(ValidModel());

        var reads = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => service.GetUsersAsync("Ada")));
        Assert.All(reads, users => Assert.Single(users));

        await using (var otherContext = database.CreateDbContext())
        {
            var user = await otherContext.Users.SingleAsync();
            user.FirstName = "Augusta";
            await otherContext.SaveChangesAsync();
        }

        Assert.Equal("Augusta", (await service.GetByIdAsync(original.Id))?.FirstName);
        Assert.Empty(await service.GetUsersAsync("Ada"));
    }

    private static UserFormModel ValidModel() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Email = "ada.lovelace@example.com"
    };

    private static string Quote(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
