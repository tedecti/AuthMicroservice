using System.Security.Claims;
using AuthMicroservice.Models.Users;
using AuthMicroservice.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private static readonly Random random = new Random();

        public AuthController(IAuthRepository authRepository, IUserRepository userRepository)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                var newUser = await _authRepository.Register(user);
                return Ok(newUser);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            try
            {
                var user = await _authRepository.Login(userDto);
                if (user == null)
                {
                    return Unauthorized();
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        [HttpGet("signin-google")]
        public IActionResult SignInWithGoogle()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
                return BadRequest("Google authentication failed");

            var claims = authenticateResult.Principal?.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (email == null || name == null)
                return BadRequest("Unable to retrieve user information from Google");

            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Name = name,
                    Password = BCrypt.Net.BCrypt.HashPassword(RandomString(10)),
                    UserType = "Seller"
                };
                await _authRepository.Register(user);
            }

            var token = _authRepository.GenerateJwtToken(user);

            return Ok(new { Token = token });
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
    
}