// JwtService.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SQL_SERVER.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly int _expirationInSeconds;

        public JwtService(string secret, int expirationInSeconds)
        {
            _secret = secret;
            _expirationInSeconds = expirationInSeconds;
        }

        public string GenerateToken(Guid userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", userId.ToString()),
                    new Claim("email", email)
                }),
                Expires = DateTime.UtcNow.AddSeconds(_expirationInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret); // ✅ FIXED: Use _secret instead of secret
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken validatedToken); // ✅ FIXED: Added variable name

                // ✅ DEBUG: Log all claims found in the token
                Console.WriteLine($"[JwtService] Token validation successful, found {principal.Claims.Count()} claims:");
                foreach (var claim in principal.Claims)
                {
                    Console.WriteLine($"[JwtService] Claim: {claim.Type} = '{claim.Value}'");
                }

                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JwtService] Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
}