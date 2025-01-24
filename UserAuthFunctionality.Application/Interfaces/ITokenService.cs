using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Core.Entities;

namespace UserAuthFunctionality.Application.Interfaces
{
    public interface ITokenService
    {
        string GetToken(string SecretKey, string Audience, string Issuer, AppUser existUser, IList<string> roles);
        ClaimsPrincipal ValidateToken(string token);
        string GenerateRefreshToken();

    }
}
