using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities;

namespace UserAuthFunctionality.Application.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GetToken(string SecretKey, string Audience, string Issuer, AppUser existUser, IList<string> roles)
        {
            var handler = new JwtSecurityTokenHandler();
            var privateKey = Encoding.UTF8.GetBytes(SecretKey);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256);
            var ci = new ClaimsIdentity();

            ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, existUser.Id));
            ci.AddClaim(new Claim(ClaimTypes.Name, existUser.UserName));
            ci.AddClaim(new Claim(ClaimTypes.GivenName, existUser.fullName));
            ci.AddClaim(new Claim(ClaimTypes.Email, existUser.Email));

            ci.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList());
            //foreach (var role in roles)
            //    ci.AddClaim(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = credentials,
                Expires = DateTime.MaxValue,
                Subject = ci,
                Audience = Audience,
                Issuer = Issuer,
                NotBefore = DateTime.UtcNow,
            };
            var tokenHandiling = handler.CreateToken(tokenDescriptor);
            var Token = handler.WriteToken(tokenHandiling);
            return Token;
        }
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4250034f-c024-41f3-a10e-f7fd5ccc0671")),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }

        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

    }
}
