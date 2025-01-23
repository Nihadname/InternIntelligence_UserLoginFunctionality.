using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Application.Settings;
using UserAuthFunctionality.Core.Entities;

namespace UserAuthFunctionality.Application.Implementations
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(JwtSettings jwtSettings, UserManager<AppUser> userManager)
        {
            _jwtSettings = jwtSettings;
            _userManager = userManager;
        }

        public async Task RegisterUser(RegisterDto registerDto)
        {

        }
    }
}
