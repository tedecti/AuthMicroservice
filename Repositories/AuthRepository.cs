using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthMicroservice.Data;
using AuthMicroservice.Models.Users;
using AuthMicroservice.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthMicroservice.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthRepository(AuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<User> Register(User user)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, 12);
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = user.Email,
            Name = user.Name,
            Password = hashedPassword,
            UserType = user.UserType
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<string?> Login(UserDto userDto)
    { 
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (user == null || string.IsNullOrEmpty(userDto.Password) || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        return token;
    }

    public string GenerateJwtToken(User user)
    {
        try
        {
            var jsonKey = _configuration.GetValue<string>("SecretKey");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jsonKey);
            if (string.IsNullOrEmpty(jsonKey))
            {
                throw new InvalidOperationException("SecretKey is missing or empty in configuration.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                    { new Claim("id", user.Id.ToString()), new Claim("role", user.UserType) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(e.ToString());
        }
    }
}