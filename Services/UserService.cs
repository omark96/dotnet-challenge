using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UserManager.Web.Data;
using UserManager.Web.Models;

namespace UserManager.Web.Services;

public sealed class UserService(IDbContextFactory<UserManagerDbContext> contextFactory) : IUserService
{
    private static readonly string[] RequiredHeaders = ["FirstName", "LastName", "Email", "IsActive"];

    public async Task<IReadOnlyList<User>> GetUsersAsync(string? searchTerm = null)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        IQueryable<User> query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(user =>
                user.FirstName.ToLower().Contains(term) ||
                user.LastName.ToLower().Contains(term));
        }

        return await query
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == id);
    }

    public async Task<User> CreateAsync(UserFormModel model)
    {
        ValidateModel(model);
        await using var db = await contextFactory.CreateDbContextAsync();
        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            IsActive = model.IsActive,
            CreatedAt = now,
            LastModifiedAt = now
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(int id, UserFormModel model)
    {
        ValidateModel(model);
        await using var db = await contextFactory.CreateDbContextAsync();
        var user = await db.Users.SingleOrDefaultAsync(existing => existing.Id == id)
            ?? throw new InvalidOperationException("The selected user could not be found.");

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.IsActive = model.IsActive;
        user.LastModifiedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        return user;
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var user = await db.Users.SingleOrDefaultAsync(existing => existing.Id == id)
            ?? throw new InvalidOperationException("The selected user could not be found.");

        user.IsActive = isActive;
        user.LastModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<CsvImportResult> ImportCsvAsync(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            throw new FormatException("The CSV file is empty.");
        }

        var rows = ParseCsv(csvContent);
        if (rows.Count == 0)
        {
            throw new FormatException("The CSV file does not contain a header row.");
        }

        var headerPositions = CreateHeaderPositions(rows[0]);
        await using var db = await contextFactory.CreateDbContextAsync();
        var importedCount = 0;
        var errors = new List<string>();

        for (var index = 1; index < rows.Count; index++)
        {
            var row = rows[index];
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var rowNumber = index + 1;
            if (row.Count != rows[0].Count)
            {
                errors.Add($"Row {rowNumber}: Expected {rows[0].Count} columns, but found {row.Count}.");
                continue;
            }

            var model = new UserFormModel
            {
                FirstName = row[headerPositions["FirstName"]],
                LastName = row[headerPositions["LastName"]],
                Email = row[headerPositions["Email"]]
            };
            var isActiveText = row[headerPositions["IsActive"]].Trim();
            var rowErrors = model.Validate().Select(error => error.ErrorMessage).ToList();

            if (!bool.TryParse(isActiveText, out var isActive))
            {
                rowErrors.Add("IsActive must be true or false.");
            }

            if (rowErrors.Count > 0)
            {
                errors.Add($"Row {rowNumber}: {string.Join(" ", rowErrors)}");
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            db.Users.Add(new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                IsActive = isActive,
                CreatedAt = now,
                LastModifiedAt = now
            });
            importedCount++;
        }

        if (importedCount > 0)
        {
            await db.SaveChangesAsync();
        }

        return new CsvImportResult(importedCount, errors);
    }

    private static void ValidateModel(UserFormModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var errors = model.Validate();
        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join(" ", errors.Select(error => error.ErrorMessage)));
        }
    }

    private static Dictionary<string, int> CreateHeaderPositions(IReadOnlyList<string> headers)
    {
        var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headers.Count; index++)
        {
            var name = headers[index].Trim();
            if (!positions.TryAdd(name, index))
            {
                throw new FormatException($"The header '{name}' is listed more than once.");
            }
        }

        var missingHeaders = RequiredHeaders.Where(header => !positions.ContainsKey(header)).ToArray();
        if (missingHeaders.Length > 0)
        {
            throw new FormatException($"The CSV file is missing these columns: {string.Join(", ", missingHeaders)}.");
        }

        return positions;
    }

    private static List<List<string>> ParseCsv(string content)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var insideQuotes = false;
        var quoteClosed = false;

        for (var index = 0; index < content.Length; index++)
        {
            var character = content[index];
            if (insideQuotes)
            {
                if (character == '"' && index + 1 < content.Length && content[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else if (character == '"')
                {
                    insideQuotes = false;
                    quoteClosed = true;
                }
                else
                {
                    field.Append(character);
                }

                continue;
            }

            if (quoteClosed && character != ',' && character != '\r' && character != '\n')
            {
                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                throw new FormatException("The CSV file contains text after a quoted field.");
            }

            if (character == '"')
            {
                if (field.Length != 0)
                {
                    throw new FormatException("A quoted field must start with a double quote.");
                }

                insideQuotes = true;
            }
            else if (character == ',')
            {
                AddField();
                quoteClosed = false;
            }
            else if (character is '\r' or '\n')
            {
                if (character == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
                {
                    index++;
                }

                AddField();
                AddRow();
                quoteClosed = false;
            }
            else
            {
                field.Append(character);
            }
        }

        if (insideQuotes)
        {
            throw new FormatException("The CSV file contains an unclosed quoted field.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            AddField();
            AddRow();
        }

        return rows;

        void AddField()
        {
            row.Add(field.ToString());
            field.Clear();
        }

        void AddRow()
        {
            rows.Add(row);
            row = [];
        }
    }
}
