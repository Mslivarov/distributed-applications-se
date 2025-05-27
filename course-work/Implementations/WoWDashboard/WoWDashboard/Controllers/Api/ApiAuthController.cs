using Microsoft.AspNetCore.Mvc;
using System.Text;
using WoWDashboard.Data;
using WoWDashboard.Dtos;
using WoWDashboard.Models;
using WoWDashboard.Services;
using System.Security.Cryptography;

namespace WoWDashboard.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(ApplicationDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [Route("api/[controller]")]
        [HttpPost("login")]
        public IActionResult Login([FromBody] Login login)
        {
            string hashedInput = Convert.ToBase64String(
        SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(login.Password)));

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == login.Username && u.PasswordHash == hashedInput);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = _jwtTokenService.GenerateJwtToken(user);
            return Ok(new { token });
        }
    }
}
