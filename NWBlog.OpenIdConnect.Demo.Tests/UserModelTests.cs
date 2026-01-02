using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NWBlog.OpenIdConnect.Demo.Identity;
using Xunit;

namespace NWBlog.OpenIdConnect.Demo.Tests
{
    /// <summary>
    /// Demo Test 2: User Model Tests
    /// Tests the User entity model validation and properties
    /// </summary>
    public class UserModelTests
    {
        [Fact]
        public void User_ShouldInitializeWithDefaultId()
        {
            // Arrange & Act
            var user = new User
            {
                Username = "testuser",
                PasswordHash = "hash"
            };

            // Assert
            Assert.NotEqual(Guid.Empty, user.Id);
        }

        [Fact]
        public void User_ShouldHaveRequiredUsername()
        {
            // Arrange
            var user = new User
            {
                Username = null,
                PasswordHash = "hash"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(user);
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.Username)));
        }

        [Fact]
        public void User_ShouldHaveRequiredPasswordHash()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                PasswordHash = null
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(user);
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(User.PasswordHash)));
        }

        [Fact]
        public void User_ShouldBeValid_WhenAllRequiredFieldsAreProvided()
        {
            // Arrange
            var user = new User
            {
                Username = "validuser",
                PasswordHash = "validhash123"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(user);
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void User_ShouldAllowUserRolesCollection()
        {
            // Arrange & Act
            var user = new User
            {
                Username = "testuser",
                PasswordHash = "hash",
                UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = Guid.NewGuid(), RoleId = 1 }
                }
            };

            // Assert
            Assert.NotNull(user.UserRoles);
            Assert.Single(user.UserRoles);
        }

        [Fact]
        public void User_Properties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "propertytest";
            var passwordHash = "testHash123";

            // Act
            var user = new User
            {
                Id = userId,
                Username = username,
                PasswordHash = passwordHash
            };

            // Assert
            Assert.Equal(userId, user.Id);
            Assert.Equal(username, user.Username);
            Assert.Equal(passwordHash, user.PasswordHash);
        }
    }
}