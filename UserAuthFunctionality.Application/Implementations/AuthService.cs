using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.AppDefaults;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Application.Helper.Enums;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Application.Settings;
using UserAuthFunctionality.Core.Entities;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Implementations
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        public AuthService(IOptions<JwtSettings> jwtSettings, UserManager<AppUser> userManager, IPhotoService photoService, IMapper mapper, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, ITokenService tokenService)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _photoService = photoService;
            _mapper = mapper;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }

        public async Task<Result<Task>> RegisterUser(RegisterDto registerDto)
        {
            var existUser = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existUser != null) return Result<Task>.Failure("UserName", "UserName is already Taken", ErrorType.BusinessLogicError);
            var existUserEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existUserEmail != null)
                return Result<Task>.Failure("Email", "Email is already taken", ErrorType.BusinessLogicError);
            if (await _userManager.Users.FirstOrDefaultAsync(s => s.PhoneNumber.ToLower() == registerDto.PhoneNumber.ToLower()) is not null)
            {
                return Result<Task>.Failure("PhoneNumber", "PhoneNumber already exists", ErrorType.BusinessLogicError);
            }
            AppUser appUser = new AppUser();
            appUser.UserName = registerDto.UserName;
            appUser.Email = registerDto.Email;
            appUser.fullName = registerDto.FullName;
            appUser.PhoneNumber = registerDto.PhoneNumber;
            appUser.Image = AppDefaultValue.DefaultProfileImageUrl;
            appUser.CreatedTime = DateTime.UtcNow;
            appUser.BirthDate = registerDto.BirthDate;
            var result = await _userManager.CreateAsync(appUser, registerDto.Password);
            if (!result.Succeeded)
            {
                string errors = string.Empty;
                var errorMessages = result.Errors.ToDictionary(e => e.Code, e => e.Description);
                foreach (KeyValuePair<string, string> keyValues in errorMessages)
                {
                    errors += keyValues.Key + " : " + keyValues.Value + ", ";
                }
                return Result<Task>.Failure(null, errors.TrimEnd(',', ' '), ErrorType.SystemError);
            }
            await _userManager.AddToRoleAsync(appUser, AppUserRoleEnum.Member.ToString());
            return Result<Task>.Success(Task.CompletedTask);

        }
        public async Task<Result<AuthLoginResponseDto>> Login(LoginDto loginDto)
        {
            var User = await _userManager.FindByEmailAsync(loginDto.UserNameOrGmail);
            if (User == null)
            {
                User = await _userManager.FindByNameAsync(loginDto.UserNameOrGmail);

                if (User == null)
                {
                    return Result<AuthLoginResponseDto>.Failure("Login", "userName or email or password is wrong\"", ErrorType.NotFoundError);
                }
            }
            var result = await _userManager.CheckPasswordAsync(User, loginDto.Password);
            if (!result)
            {
                return Result<AuthLoginResponseDto>.Failure("Login", "userName or email or password is wrong", ErrorType.ValidationError);
            }
            if (User.IsBlocked && User.BlockedUntil.HasValue)
            {
                if (User.BlockedUntil.Value <= DateTime.UtcNow)
                {
                    User.IsBlocked = false;
                    User.BlockedUntil = null;
                    await _userManager.UpdateAsync(User);
                }
                else
                {

                    return Result<AuthLoginResponseDto>.Failure("UserNameOrGmail", $"you are blocked until {User.BlockedUntil?.ToString("dd MMM yyyy hh:mm")}", ErrorType.BusinessLogicError);
                }
            }
            IList<string> roles = await _userManager.GetRolesAsync(User);
            var Audience = _jwtSettings.Audience;
            var SecretKey = _jwtSettings.secretKey;
            var Issuer = _jwtSettings.Issuer;
            return Result<AuthLoginResponseDto>.Success(new AuthLoginResponseDto
            {
                IsLogined = true,
                Token = _tokenService.GetToken(SecretKey, Audience, Issuer, User, roles)
            });
        }
        public async Task<Result<bool>> ValidateToken( string Authorization)
        {
            if (string.IsNullOrEmpty(Authorization) || !Authorization.StartsWith("Bearer "))
            {
                return Result<bool>.Failure(null, "Invalid token format", ErrorType.UnauthorizedError);
            }
            var token = Authorization.Substring("Bearer ".Length).Trim();
            var principal = _tokenService.ValidateToken(token);
            if(principal == null) return Result<bool>.Failure(null, "Invalid or expired token",ErrorType.UnauthorizedError);
            var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<bool>.Failure("Id", "User ID cannot be null", ErrorType.UnauthorizedError);
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                 return Result<bool>.Failure("User", "User  cannot be null", ErrorType.UnauthorizedError);
            }
          return  Result<bool>.Success(true);

        }
        //public async Task<string> AddRole()
        //{
        //    if (!await _roleManager.RoleExistsAsync("Admin"))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
        //    }
        //    if (!await _roleManager.RoleExistsAsync("Member"))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole() { Name = "Member" });
        //    }
        //    return "added role";
        //}
    }
}
