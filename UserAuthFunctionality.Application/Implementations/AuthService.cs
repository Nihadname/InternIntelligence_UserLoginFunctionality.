using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using UserAuthFunctionality.Application.AppDefaults;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Application.Extensions;
using UserAuthFunctionality.Application.Helper.Enums;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Application.Settings;
using UserAuthFunctionality.Core.Entities;
using UserAuthFunctionality.Core.Entities.Common;
using UserAuthFunctionality.DataAccess.Data;

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
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IEmailService _emailService;
        public AuthService(IOptions<JwtSettings> jwtSettings, UserManager<AppUser> userManager, IPhotoService photoService, IMapper mapper, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, ApplicationDbContext applicationDbContext, IEmailService emailService)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _photoService = photoService;
            _mapper = mapper;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
            _applicationDbContext = applicationDbContext;
            _emailService = emailService;
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
            await SendVerificationCode(appUser.Email);
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
            if(!User.IsEmailVerificationCodeValid) return Result<AuthLoginResponseDto>.Failure("VerifyCode", "VerificationCode is not accepted yet ", ErrorType.BusinessLogicError);
            IList<string> roles = await _userManager.GetRolesAsync(User);
            var Audience = _jwtSettings.Audience;
            var SecretKey = _jwtSettings.secretKey;
            var Issuer = _jwtSettings.Issuer;
            var refreshTokenGenerated = _tokenService.GenerateRefreshToken();
            RefreshToken refreshToken= new RefreshToken { AppUser = User , AppUserId=User.Id,Token = refreshTokenGenerated,Expires= DateTime.UtcNow.AddDays(7) };
            if (User.RefreshTokens == null)
            {
                User.RefreshTokens = new List<RefreshToken>();
            }
            User.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(User);
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshTokenGenerated, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Result<AuthLoginResponseDto>.Success(new AuthLoginResponseDto
            {
                IsLogined = true,
                Token = _tokenService.GetToken(SecretKey, Audience, Issuer, User, roles),
                RefreshToken= refreshToken.Token,
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
        public async Task<Result<AuthLoginResponseDto>> RefreshToken()
        {
            
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

            if(string.IsNullOrEmpty(refreshToken))
                return Result<AuthLoginResponseDto>.Failure("RefreshToken", "Refresh token is missing", ErrorType.UnauthorizedError);
            var refreshTokenFetchingFromDatabase = await _applicationDbContext
                .refreshTokens
                .Include(s=>s.AppUser)
                .OrderByDescending(s=>s.Expires)
                .FirstOrDefaultAsync(s=>s.Token == refreshToken);
            if(refreshTokenFetchingFromDatabase == null)
                return Result<AuthLoginResponseDto>.Failure("RefreshToken", "Invalid refresh token", ErrorType.UnauthorizedError);

            if (!refreshTokenFetchingFromDatabase.IsActive)
                return Result<AuthLoginResponseDto>.Failure("RefreshToken", "Invalid or expired refresh token", ErrorType.UnauthorizedError);
            var user = refreshTokenFetchingFromDatabase.AppUser;
            if (user == null)
                return Result<AuthLoginResponseDto>.Failure("Id", "User does not exist", ErrorType.UnauthorizedError);

            IList<string> roles = await _userManager.GetRolesAsync(user);
            var Audience = _jwtSettings.Audience;
            var SecretKey = _jwtSettings.secretKey;
            var Issuer = _jwtSettings.Issuer;
            var newrefreshTokenGenerated = _tokenService.GenerateRefreshToken();
            var newAccessToken = _tokenService.GetToken(SecretKey, Audience, Issuer, user, roles);
            RefreshToken refreshTokenAsObject = new RefreshToken {AppUserId = user.Id, Token = newrefreshTokenGenerated, Expires = DateTime.UtcNow.AddDays(7) };

           await _applicationDbContext.refreshTokens.AddAsync(refreshTokenAsObject);
            await _applicationDbContext.SaveChangesAsync();

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", newrefreshTokenGenerated, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Result<AuthLoginResponseDto>.Success(new AuthLoginResponseDto { Token = newAccessToken, RefreshToken= refreshTokenAsObject.Token });

        }
        public async Task<Result<string>> UpdateImage(UserUpdateImageDto userUpdateImageDto)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Result<string>.Failure("Id", "User Id cannot be null", ErrorType.UnauthorizedError);
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<string>.Failure("Id", "this user doesnt exist", ErrorType.UnauthorizedError);
            if (!string.IsNullOrEmpty(user.Image))
            {
                await _photoService.DeletePhotoAsync(user.Image);
            }
            user.Image = await _photoService.UploadPhotoAsync(userUpdateImageDto.Image);
            await _userManager.UpdateAsync(user);
            return Result<string>.Success(user.Image);
        }
        public async Task<Result<string>> SendVerificationCode(string email)
        {
            if (string.IsNullOrEmpty(email)) return Result<string>.Failure("email", "email is null", ErrorType.ValidationError);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return Result<string>.Failure("user", "user is null", ErrorType.NotFoundError);
            var verificationCode = new Random().Next(100000, 999999).ToString();
            string salt;
            string hashedCode = verificationCode.GenerateHash(out salt);
            user.VerificationCode = hashedCode;
            user.Salt = salt;
            user.ExpiredDate = DateTime.UtcNow.AddMinutes(10);
            user.IsEmailVerificationCodeValid = false;
            await _userManager.UpdateAsync(user);
            var body = $"<h1>Welcome!</h1><p>Thank you for joining us. We're excited to have you!, this is your verfication code {verificationCode} </p>";
            _emailService.SendEmail(body, email, "Verify Code", "Verify Code");
            return Result<string>.Success("Verification code sent");

        }
        public async Task<Result<string>> VerifyCode(VerifyCodeDto verifyCodeDto)
        {
            var existedUser = await _userManager.FindByEmailAsync(verifyCodeDto.Email);
            if (existedUser is null) return Result<string>.Failure("User", "User is null", ErrorType.NotFoundError);
            bool isValid = HashExtension.VerifyHash(verifyCodeDto.Code, existedUser.Salt, existedUser.VerificationCode);
            if (!isValid || existedUser.ExpiredDate < DateTime.UtcNow)
                return Result<string>.Failure("Code", "Invalid or expired verification code.", ErrorType.BusinessLogicError);
            existedUser.IsEmailVerificationCodeValid = true;
            existedUser.VerificationCode = null;
            existedUser.ExpiredDate = null;
            await _userManager.UpdateAsync(existedUser);
            return Result<string>.Success("Code verified successfully. You can now log in.");
        }
        public async  Task<Result<UserGetDto>> Profile()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Result<UserGetDto>.Failure("Id", "User ID cannot be null", ErrorType.UnauthorizedError);
            }
            var existedUser = await _userManager.FindByIdAsync(userId);
            if (existedUser == null)
            {
                if (existedUser is null) return Result<UserGetDto>.Failure("Id", "User ID cannot be null", ErrorType.UnauthorizedError);

            }
            var mappedUser = _mapper.Map<UserGetDto>(existedUser);
            return Result<UserGetDto>.Success(mappedUser);
        }
        public async Task<Result<string>> RevokeRefreshToken()
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Result<string>.Failure("RefreshToken", "Refresh token is missing", ErrorType.UnauthorizedError);
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Result<string>.Failure("Id", "User ID cannot be null", ErrorType.UnauthorizedError);
            }
            var existedUser = await _userManager.FindByIdAsync(userId);
            if (existedUser == null)
            {
                if (existedUser is null) return Result<string>.Failure("Id", "User ID cannot be null", ErrorType.UnauthorizedError);
            }
            var existedRefreshToken=await _applicationDbContext.refreshTokens.Include(s=>s.AppUser).FirstOrDefaultAsync(s=>s.Token==refreshToken&&s.IsActive);
            if (existedRefreshToken == null) return Result<string>.Failure("Id", "id with this  refresh token doesnt exist",ErrorType.NotFoundError);
            if (existedRefreshToken.AppUser == null || existedRefreshToken.AppUser == existedUser)
                return Result<string>.Failure("User", "User doesnt match or exists",ErrorType.NotFoundError);
            existedRefreshToken.Revoked = DateTime.UtcNow;
             _applicationDbContext.refreshTokens.Update(existedRefreshToken);
            await _applicationDbContext.SaveChangesAsync();
            return Result<string>.Success("revoked Token");

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
