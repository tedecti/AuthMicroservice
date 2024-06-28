using AuthMicroservice.Models.Users;

namespace AuthMicroservice.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<User> Register(User user);
    Task<string?> Login(UserDto userDto);
}