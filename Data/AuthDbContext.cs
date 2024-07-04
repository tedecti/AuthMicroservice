using AuthMicroservice.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }
    public AuthDbContext() {}

    public DbSet<User> Users { get; set; } = default!;
}