using UserManager.Web.Models;

namespace UserManager.Tests;

public sealed class UserFormModelTests
{
    [Fact]
    public void ContactDetails_AreTrimmedBeforeValidation()
    {
        var model = new UserFormModel
        {
            FirstName = "  Ada ",
            LastName = "\tLovelace ",
            Email = " ada@example.com "
        };

        Assert.Equal("Ada", model.FirstName);
        Assert.Equal("Lovelace", model.LastName);
        Assert.Equal("ada@example.com", model.Email);
        Assert.Empty(model.Validate());
    }

    [Fact]
    public void Validate_AllowsMaximumLengthsAfterTrimming()
    {
        var model = new UserFormModel
        {
            FirstName = " " + new string('A', 80) + " ",
            LastName = " " + new string('L', 80) + " ",
            Email = " " + new string('a', 242) + "@example.com "
        };

        Assert.Equal(254, model.Email.Length);
        Assert.Empty(model.Validate());
    }

    [Fact]
    public void Validate_IdentifiesEachMissingField()
    {
        var errors = new UserFormModel().Validate();

        Assert.Equal(3, errors.Count);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserFormModel.FirstName)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserFormModel.LastName)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(UserFormModel.Email)));
    }
}
