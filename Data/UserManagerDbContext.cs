using Microsoft.EntityFrameworkCore;
using UserManager.Web.Models;

namespace UserManager.Web.Data;

public sealed class UserManagerDbContext(DbContextOptions<UserManagerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
        });
    }
}
