using Microsoft.EntityFrameworkCore;
using NWBlog.OpenIdConnect.Demo.Identity;

namespace NWBlog.OpenIdConnect.Demo
{
    public class DefaultDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.RoleId, ur.UserId });
            modelBuilder.Entity<UserRole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        }
    }
}
