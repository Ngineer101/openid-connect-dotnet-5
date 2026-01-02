using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWBlog.OpenIdConnect.Demo;
using NWBlog.OpenIdConnect.Demo.Identity;
using Xunit;

namespace NWBlog.OpenIdConnect.Demo.Tests
{
    /// <summary>
    /// Demo Test 1: UserStore Tests
    /// Tests the custom UserStore implementation for creating and retrieving users
    /// </summary>
    public class UserStoreTests
    {
        private DefaultDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DefaultDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new DefaultDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);
            var user = new User
            {
                Username = "testuser",
                PasswordHash = "hashedpassword123"
            };

            // Act
            var result = await userStore.CreateAsync(user, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser.Username);
            Assert.Equal("hashedpassword123", savedUser.PasswordHash);
        }

        [Fact]
        public async Task FindByNameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);
            var user = new User
            {
                Username = "existinguser",
                PasswordHash = "password123"
            };
            await userStore.CreateAsync(user, CancellationToken.None);

            // Act
            var foundUser = await userStore.FindByNameAsync("existinguser", CancellationToken.None);

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal("existinguser", foundUser.Username);
            Assert.Equal("password123", foundUser.PasswordHash);
        }

        [Fact]
        public async Task FindByNameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            // Act
            var foundUser = await userStore.FindByNameAsync("nonexistentuser", CancellationToken.None);

            // Assert
            Assert.Null(foundUser);
        }

        [Fact]
        public async Task GetPasswordHashAsync_ShouldReturnCorrectHash()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                PasswordHash = "expectedhash"
            };
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            // Act
            var passwordHash = await userStore.GetPasswordHashAsync(user, CancellationToken.None);

            // Assert
            Assert.Equal("expectedhash", passwordHash);
        }

        [Fact]
        public async Task GetUserNameAsync_ShouldReturnUsername()
        {
            // Arrange
            var user = new User
            {
                Username = "myusername",
                PasswordHash = "somehash"
            };
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            // Act
            var username = await userStore.GetUserNameAsync(user, CancellationToken.None);

            // Assert
            Assert.Equal("myusername", username);
        }

        [Fact]
        public async Task SetNormalizedUserNameAsync_ShouldUpdateUsername()
        {
            // Arrange
            var user = new User
            {
                Username = "oldname",
                PasswordHash = "hash"
            };
            using var context = CreateInMemoryContext();
            var userStore = new UserStore(context);

            // Act
            await userStore.SetNormalizedUserNameAsync(user, "NEWNAME", CancellationToken.None);

            // Assert
            Assert.Equal("NEWNAME", user.Username);
        }
    }
}