using UserManager.Web.Models;

namespace UserManager.Web.Services;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetUsersAsync(string? searchTerm = null);

    Task<User?> GetByIdAsync(int id);

    Task<User> CreateAsync(UserFormModel model);

    Task<User> UpdateAsync(int id, UserFormModel model);

    Task SetActiveAsync(int id, bool isActive);

    Task<CsvImportResult> ImportCsvAsync(string csvContent);
}
