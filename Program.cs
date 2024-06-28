using AuthMicroservice.Data;
using AuthMicroservice.Repositories;
using AuthMicroservice.Repositories.Interfaces;

namespace AuthMicroservice;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://localhost:8081");
        builder.Services.AddDbContext<AuthDbContext>();
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}