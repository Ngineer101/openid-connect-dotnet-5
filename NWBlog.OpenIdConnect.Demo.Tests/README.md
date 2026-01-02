# Demo Tests for OpenID Connect .NET 5

This test project contains 4 demo test files demonstrating different testing approaches for the OpenID Connect implementation.

## Test Files

### 1. UserStoreTests.cs
Tests the custom `UserStore` implementation which handles user persistence and retrieval.

**Tests include:**
- Creating users in the database
- Finding users by username
- Handling non-existent users
- Getting password hashes
- Getting usernames
- Setting normalized usernames

**Key Features:**
- Uses Entity Framework Core InMemory database for isolated testing
- Tests core Identity framework integration points

### 2. UserModelTests.cs
Tests the `User` entity model validation and properties.

**Tests include:**
- Default ID generation
- Required field validation (Username, PasswordHash)
- Valid user creation
- User roles collection handling
- Property getters and setters

**Key Features:**
- Uses Data Annotations validation
- Demonstrates model validation patterns

### 3. AuthenticationControllerTests.cs
Tests the authentication endpoint with mocked dependencies.

**Tests include:**
- Unauthorized response when user not found
- Unauthorized response when password is invalid
- Bad request for unsupported grant types
- Controller initialization
- Password grant type handling

**Key Features:**
- Uses Moq for mocking UserManager and SignInManager
- Tests controller behavior without database dependencies
- Demonstrates unit testing of ASP.NET Core controllers

### 4. DatabaseIntegrationTests.cs
Integration tests for the complete database workflow including UserStore, DbContext, and entities.

**Tests include:**
- Complete user workflow with roles
- Unique username constraint enforcement
- Multiple user persistence
- User-role relationship integrity
- Automatic ID generation
- Username normalization and persistence

**Key Features:**
- Full integration testing with Entity Framework Core
- Tests relationships between User, Role, and UserRole entities
- Demonstrates real database operations with InMemory provider

## Running the Tests

### Run all tests:
```bash
dotnet test
```

### Run tests from a specific file:
```bash
dotnet test --filter "FullyQualifiedName~UserStoreTests"
dotnet test --filter "FullyQualifiedName~UserModelTests"
dotnet test --filter "FullyQualifiedName~AuthenticationControllerTests"
dotnet test --filter "FullyQualifiedName~DatabaseIntegrationTests"
```

### Run a specific test:
```bash
dotnet test --filter "FullyQualifiedName~CreateAsync_ShouldAddUserToDatabase"
```

## Test Coverage

These demo tests cover:
- ✅ User model validation
- ✅ UserStore CRUD operations
- ✅ Authentication controller endpoints
- ✅ Database relationships and constraints
- ✅ Identity framework integration

## Technologies Used

- **xUnit** - Testing framework
- **Moq** - Mocking library for unit tests
- **Entity Framework Core InMemory** - In-memory database for testing
- **ASP.NET Core Identity** - User management framework

## Notes

- The AuthenticationController tests mock dependencies because full OpenIddict integration requires complex setup
- The DatabaseIntegrationTests use EF Core InMemory provider which has some limitations compared to real SQL databases
- Some UserStore methods throw `NotImplementedException` as they are not yet implemented in the main project