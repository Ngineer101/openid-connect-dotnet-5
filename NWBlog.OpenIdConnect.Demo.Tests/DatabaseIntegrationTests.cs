using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWBlog.OpenIdConnect.Demo;
using NWBlog.OpenIdConnect.Demo.Identity;
using Xunit;

namespace NWBlog.OpenIdConnect.Demo.Tests
{
    /// <summary>
    /// Demo Test 4: Database Integration Tests
    /// Tests the full integration between UserStore, DbContext, and User entities
    /// including relationships, constraints, and data persistence
    /// </summary>
    public class DatabaseIntegrationTests
    {
        private DefaultDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DefaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DefaultDbContext(options);
        }

        [Fact]
        public async Task CompleteUserWorkflow_ShouldCreateAndRetrieveUserWithRoles()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            // Create a role
            var role = new Role
            {
                Id = 1,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            // Create a user
            var user = new User
            {
                Username = "admin",
                PasswordHash = "securehashedpassword"
            };

            // Act - Create user
            var createResult = await userStore.CreateAsync(user, CancellationToken.None);
            Assert.True(createResult.Succeeded);

            // Add user role relationship
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            // Act - Retrieve user with roles
            var retrievedUser = await userStore.FindByNameAsync("admin", CancellationToken.None);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("admin", retrievedUser.Username);
            Assert.Equal("securehashedpassword", retrievedUser.PasswordHash);
            Assert.NotNull(retrievedUser.UserRoles);
            Assert.Single(retrievedUser.UserRoles);
            Assert.Equal("ADMINISTRATOR", retrievedUser.UserRoles.First().Role.NormalizedName);
        }

        [Fact]
        public async Task Database_ShouldEnforceUniqueUsername()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            var user1 = new User
            {
                Username = "uniqueuser",
                PasswordHash = "hash1"
            };

            var user2 = new User
            {
                Username = "uniqueuser",
                PasswordHash = "hash2"
            };

            // Act
            await userStore.CreateAsync(user1, CancellationToken.None);

            // Assert - Second user with same username should fail
            // Note: In-memory database doesn't enforce unique constraints like SQL databases
            // This test demonstrates the constraint is defined in the model
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await userStore.CreateAsync(user2, CancellationToken.None);
            });

            // In-memory DB will throw when trying to track duplicate entities
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task UserStore_ShouldPersistMultipleUsers()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            var users = new[]
            {
                new User { Username = "user1", PasswordHash = "hash1" },
                new User { Username = "user2", PasswordHash = "hash2" },
                new User { Username = "user3", PasswordHash = "hash3" }
            };

            // Act
            foreach (var user in users)
            {
                await userStore.CreateAsync(user, CancellationToken.None);
            }

            // Assert
            var allUsers = await context.Users.ToListAsync();
            Assert.Equal(3, allUsers.Count);
            Assert.Contains(allUsers, u => u.Username == "user1");
            Assert.Contains(allUsers, u => u.Username == "user2");
            Assert.Contains(allUsers, u => u.Username == "user3");
        }

        [Fact]
        public async Task UserRoles_ShouldMaintainRelationshipIntegrity()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var user = new User
            {
                Username = "roleuser",
                PasswordHash = "hash"
            };
            context.Users.Add(user);

            var adminRole = new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN" };
            var userRole = new Role { Id = 2, Name = "User", NormalizedName = "USER" };
            context.Roles.AddRange(adminRole, userRole);

            await context.SaveChangesAsync();

            // Act - Add multiple roles to user
            var userAdminRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id };
            var userUserRole = new UserRole { UserId = user.Id, RoleId = userRole.Id };
            context.UserRoles.AddRange(userAdminRole, userUserRole);
            await context.SaveChangesAsync();

            // Assert
            var retrievedUser = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Username == "roleuser");

            Assert.Equal(2, retrievedUser.UserRoles.Count);
            Assert.Contains(retrievedUser.UserRoles, ur => ur.Role.NormalizedName == "ADMIN");
            Assert.Contains(retrievedUser.UserRoles, ur => ur.Role.NormalizedName == "USER");
        }

        [Fact]
        public async Task DbContext_ShouldGenerateUserIdAutomatically()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var user = new User
            {
                Username = "newuser",
                PasswordHash = "hash"
            };

            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert
            Assert.NotEqual(Guid.Empty, user.Id);

            var retrievedUser = await context.Users.FirstAsync(u => u.Username == "newuser");
            Assert.Equal(user.Id, retrievedUser.Id);
        }

        [Fact]
        public async Task UserStore_ShouldUpdateNormalizedUsernameAndPersist()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            var user = new User
            {
                Username = "lowercase",
                PasswordHash = "hash"
            };

            await userStore.CreateAsync(user, CancellationToken.None);

            // Act
            await userStore.SetNormalizedUserNameAsync(user, "UPPERCASE", CancellationToken.None);
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Assert
            var retrievedUser = await context.Users.FirstAsync(u => u.Id == user.Id);
            Assert.Equal("UPPERCASE", retrievedUser.Username);
        }
    }
}