using AuthMicroservice.Models.Users;
using AuthMicroservice.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var newUser = await _authRepository.Register(user);
            return Ok(newUser);
        }
        
        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            var user = await _authRepository.Login(userDto);
            return Ok(user);
        }
    }
}
