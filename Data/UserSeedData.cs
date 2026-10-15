using UserManager.Web.Models;

namespace UserManager.Web.Data;

public static class UserSeedData
{
    public static IReadOnlyList<User> Create()
    {
        var now = DateTimeOffset.UtcNow;

        return
        [
            CreateUser("Anna", "Lind", "anna.lind@example.com", true, now.AddDays(-58), now.AddDays(-6)),
            CreateUser("Erik", "Åström", "erik.astrom@example.com", true, now.AddDays(-52), now.AddDays(-2)),
            CreateUser("Linnea", "Äberg", "linnea.aberg@example.com", true, now.AddDays(-47), now.AddDays(-4)),
            CreateUser("Maja", "Östberg", "maja.ostberg@example.com", false, now.AddDays(-41), now.AddDays(-12)),
            CreateUser("Sofia", "Müller", "sofia.muller@example.com", true, now.AddDays(-36), now.AddDays(-1)),
            CreateUser("João", "Pereira", "joao.pereira@example.com", false, now.AddDays(-30), now.AddDays(-9)),
            CreateUser("Aisha", "Khan", "aisha.khan@example.com", true, now.AddDays(-25), now.AddDays(-3)),
            CreateUser("Mateo", "García", "mateo.garcia@example.com", true, now.AddDays(-20), now.AddDays(-5)),
            CreateUser("Chloé", "Dubois", "chloe.dubois@example.com", false, now.AddDays(-16), now.AddDays(-8)),
            CreateUser("Omar", "Haddad", "omar.haddad@example.com", true, now.AddDays(-12), now.AddDays(-2)),
            CreateUser("Yuki", "Tanaka", "yuki.tanaka@example.com", true, now.AddDays(-8), now.AddDays(-1)),
            CreateUser("Fatima", "El-Sayed", "fatima.elsayed@example.com", false, now.AddDays(-4), now.AddDays(-1))
        ];
    }

    private static User CreateUser(
        string firstName,
        string lastName,
        string email,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset lastModifiedAt) =>
        new()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = isActive,
            CreatedAt = createdAt,
            LastModifiedAt = lastModifiedAt
        };
}
