using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LibraryMemberSystem.Application.Interfaces;

namespace LibraryMemberSystem.Application.Services;

public class TokenService : ITokenService
{
   private readonly IConfiguration _config;

   public TokenService(IConfiguration config) => _config = config;

   public string GenerateToken(Guid userId, string username, string role)
   {
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new[]
      {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, role)
        };

      var token = new JwtSecurityToken(
          issuer: _config["Jwt:Issuer"],
          audience: _config["Jwt:Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
          signingCredentials: creds);

      return new JwtSecurityTokenHandler().WriteToken(token);
   }
}