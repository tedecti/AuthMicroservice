using AuthMicroservice.Data;
using AuthMicroservice.Models.Users;
using AuthMicroservice.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthMicroservice.Tests;

public class AuthRepositoryTest
{
    private readonly Mock<AuthDbContext> _mockContext;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockConfigurationSection;
    private readonly AuthRepository _authRepository;

    public AuthRepositoryTest()
    {
        _mockConfigurationSection = new Mock<IConfigurationSection>();
        _mockContext = new Mock<AuthDbContext>();
        _mockConfiguration = new Mock<IConfiguration>();
        _authRepository = new AuthRepository(_mockContext.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task Register_AddsUserToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        var context = new AuthDbContext(options);

        // Act
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "password",
            UserType = "User"
        };
        var repository = new AuthRepository(context, _mockConfiguration.Object);
        var result = await repository.Register(user);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Name, result.Name);
        Assert.NotEqual(user.Password, result.Password);
        Assert.Equal(user.UserType, result.UserType);
    }

    [Fact]
    public async Task Login_ReturnsToken_ForValidCredentials()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        var context = new AuthDbContext(options);
        _mockConfigurationSection.Setup(x => x.Value)
            .Returns("mockedsecretstringofvalue128bits");
        _mockConfiguration.Setup(x => x.GetSection("SecretKey"))
            .Returns(_mockConfigurationSection.Object);
        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Password = BCrypt.Net.BCrypt.HashPassword("password", 12),
            UserType = "User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _mockConfiguration.Setup(config => config["SecretKey"]).Returns("mockedsecretstringofvalue128bits");
        var repository = new AuthRepository(context, _mockConfiguration.Object);
        var userDto = new UserDto { Email = "test@example.com", Password = "password" };
        var result = await repository.Login(userDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public async Task Login_ReturnsNull_ForInvalidEmail()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        var context = new AuthDbContext(options);

        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Password = BCrypt.Net.BCrypt.HashPassword("password", 12),
            UserType = "Seller"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userDto = new UserDto { Email = "invalid@example.com", Password = "password" };
        var repository = new AuthRepository(context, _mockConfiguration.Object);
        var findValidUser = await context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        var findInvalidUser = await context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);

        var result = await repository.Login(userDto);

        // Assert
        Assert.Null(result);
        Assert.NotNull(findValidUser);
        Assert.Null(findInvalidUser);
    }

    [Fact]
    public async Task Login_ReturnsNull_ForInvalidPassword()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        var context = new AuthDbContext(options);

        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Password = BCrypt.Net.BCrypt.HashPassword("password", 12),
            UserType = "Seller"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();


        var userDto = new UserDto { Email = "test@example.com", Password = "wrongpassword" };
        var repository = new AuthRepository(context, _mockConfiguration.Object);

        var result = await repository.Login(userDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateJwtToken_ReturnsToken_ForValidUser()
    {
        // Arrange
        _mockConfigurationSection.Setup(x => x.Value)
            .Returns("mockedsecretstringofvalue128bits");
        _mockConfiguration.Setup(x => x.GetSection("SecretKey"))
            .Returns(_mockConfigurationSection.Object);

        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "test@gmail.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password"),
            UserType = "Seller"
        };
        _mockConfiguration.Setup(config => config["SecretKey"]).Returns("mockedsecretstringofvalue128bits");

        var token = _authRepository.GenerateJwtToken(user);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
    }
}