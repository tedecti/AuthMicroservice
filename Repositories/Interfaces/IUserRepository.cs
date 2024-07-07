using AuthMicroservice.Models.Users;

namespace AuthMicroservice.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserById(Guid id);
    Task<User?> GetUserByEmail(string email);
}