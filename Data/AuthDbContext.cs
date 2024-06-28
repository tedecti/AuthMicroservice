using AuthMicroservice.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Data;

public class AuthDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public AuthDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("AuthDb"));
    }

    public DbSet<User> Users { get; set; } = default!;
}