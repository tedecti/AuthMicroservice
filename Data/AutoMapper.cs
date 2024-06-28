using AuthMicroservice.Models.Users;
using AutoMapper;

namespace AuthMicroservice.Data;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        CreateMap<User, UserDto>();
    }
}