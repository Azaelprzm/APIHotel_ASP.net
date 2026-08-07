using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelAPI.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HotelAPI.Services
{
    public class AuthService
    {
        private readonly JwtOptions _options;

        public AuthService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public GeneratedToken GenerateJwtToken(string email, string rol)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds
            );

            return new GeneratedToken(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAtUtc);
        }
    }

    public sealed record GeneratedToken(string Value, DateTime ExpiresAtUtc);
}
