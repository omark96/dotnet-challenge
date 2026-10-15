using System.ComponentModel.DataAnnotations;

namespace UserManager.Web.Models;

public sealed class UserFormModel
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(80, ErrorMessage = "First name cannot be longer than 80 characters.")]
    public string FirstName
    {
        get => _firstName;
        set => _firstName = value?.Trim() ?? string.Empty;
    }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(80, ErrorMessage = "Last name cannot be longer than 80 characters.")]
    public string LastName
    {
        get => _lastName;
        set => _lastName = value?.Trim() ?? string.Empty;
    }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Email cannot be longer than 254 characters.")]
    public string Email
    {
        get => _email;
        set => _email = value?.Trim() ?? string.Empty;
    }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<ValidationResult> Validate()
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(this, new ValidationContext(this), results, validateAllProperties: true);
        return results;
    }
}
