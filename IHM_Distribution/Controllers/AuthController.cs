using IHM_Distribution.Data.Repository;
using IHM_Distribution.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IHM_Distribution.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly IUnitOfWork _uow;

		public AuthController(IConfiguration config, IUnitOfWork uow)
		{
			_config = config;
			_uow = uow;
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserTokenDto>> Login(LoginDto loginDto)
		{
			// Find agent by pin code
			var agent = (await _uow.Agents.FindAsync(a => a.PinCode == loginDto.PinCode)).FirstOrDefault();

			if (agent == null)
				return Unauthorized("Invalid PIN code");

			// Generate token
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, agent.Id.ToString()),
                    new Claim(ClaimTypes.Name, agent.Name),
                    new Claim(ClaimTypes.Email, agent.UserEmail ?? string.Empty),
                    new Claim(ClaimTypes.Role, agent.Role ?? "Agent")
                }),
                            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryInMinutes"])),
                            Issuer = _config["JwtSettings:Issuer"],
                            Audience = _config["JwtSettings:Audience"],
                            SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);

            return new UserTokenDto
            {
                Token = tokenString,
                Name = agent.Name,
				UserEmail = agent.UserEmail,
                Expiration = token.ValidTo,
                Id = agent.Id // <-- Add this line
            };

        }
    }
}
