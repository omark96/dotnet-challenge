namespace UserManager.Web.Models;

public sealed record CsvImportResult(int ImportedCount, IReadOnlyList<string> Errors);
