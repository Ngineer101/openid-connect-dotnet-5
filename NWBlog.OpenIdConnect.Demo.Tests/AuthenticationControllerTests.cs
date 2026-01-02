using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NWBlog.OpenIdConnect.Demo.Controllers;
using NWBlog.OpenIdConnect.Demo.Identity;
using OpenIddict.Abstractions;
using Xunit;

namespace NWBlog.OpenIdConnect.Demo.Tests
{
    /// <summary>
    /// Demo Test 3: AuthenticationController Tests
    /// Tests the authentication endpoint behavior with mocked dependencies
    /// </summary>
    public class AuthenticationControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;

        public AuthenticationControllerTests()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // Setup SignInManager mock
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);
        }

        [Fact]
        public async Task TokensForPasswordGrantType_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var controller = new AuthenticationController(_mockUserManager.Object, _mockSignInManager.Object);

            // Create a mock OpenIddictRequest with password grant type
            var request = new OpenIddictRequest
            {
                GrantType = OpenIddictConstants.GrantTypes.Password,
                Username = "nonexistentuser",
                Password = "password"
            };

            // Setup HttpContext with OpenIddict request
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.SetOpenIddictServerRequest(request);

            // Act
            var result = await controller.Exchange();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockUserManager.Verify(um => um.FindByNameAsync("nonexistentuser"), Times.Once);
        }

        [Fact]
        public async Task TokensForPasswordGrantType_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword"
            };

            _mockUserManager.Setup(um => um.FindByNameAsync("testuser"))
                .ReturnsAsync(user);

            _mockSignInManager.Setup(sm => sm.CheckPasswordSignInAsync(user, "wrongpassword", false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = new AuthenticationController(_mockUserManager.Object, _mockSignInManager.Object);

            var request = new OpenIddictRequest
            {
                GrantType = OpenIddictConstants.GrantTypes.Password,
                Username = "testuser",
                Password = "wrongpassword"
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.SetOpenIddictServerRequest(request);

            // Act
            var result = await controller.Exchange();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _mockSignInManager.Verify(sm => sm.CheckPasswordSignInAsync(user, "wrongpassword", false), Times.Once);
        }

        [Fact]
        public async Task Exchange_ShouldReturnBadRequest_WhenGrantTypeIsUnsupported()
        {
            // Arrange
            var controller = new AuthenticationController(_mockUserManager.Object, _mockSignInManager.Object);

            var request = new OpenIddictRequest
            {
                GrantType = "unsupported_grant_type"
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.SetOpenIddictServerRequest(request);

            // Act
            var result = await controller.Exchange();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<OpenIddictResponse>(badRequestResult.Value);
            Assert.Equal(OpenIddictConstants.Errors.UnsupportedGrantType, response.Error);
        }

        [Fact]
        public void Controller_ShouldInitialize_WithDependencies()
        {
            // Act
            var controller = new AuthenticationController(_mockUserManager.Object, _mockSignInManager.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public async Task Exchange_ShouldHandlePasswordGrantType()
        {
            // Arrange
            var controller = new AuthenticationController(_mockUserManager.Object, _mockSignInManager.Object);

            var request = new OpenIddictRequest
            {
                GrantType = OpenIddictConstants.GrantTypes.Password,
                Username = "testuser",
                Password = "testpassword"
            };

            _mockUserManager.Setup(um => um.FindByNameAsync("testuser"))
                .ReturnsAsync((User)null);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.HttpContext.SetOpenIddictServerRequest(request);

            // Act
            var result = await controller.Exchange();

            // Assert
            Assert.NotNull(result);
            _mockUserManager.Verify(um => um.FindByNameAsync("testuser"), Times.Once);
        }
    }
}