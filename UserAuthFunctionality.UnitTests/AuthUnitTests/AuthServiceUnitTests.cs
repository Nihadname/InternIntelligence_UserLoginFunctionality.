using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using UserAuthFunctionality.Application.Interfaces;
using Moq;
using Microsoft.AspNetCore.Identity;
using UserAuthFunctionality.Core.Entities;
using Microsoft.AspNetCore.Http;
using UserAuthFunctionality.DataAccess.Data;
using UserAuthFunctionality.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using UserAuthFunctionality.Application.Implementations;
using UserAuthFunctionality.Application.Dtos.Auth;
using System.Security.Claims;

namespace UserAuthFunctionality.UnitTests.AuthUnitTests
{
    public class AuthServiceUnitTests
    {
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IPhotoService> _mockPhotoService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ApplicationDbContext> _mockApplicationDbContext;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IAuthService _authService;

        public AuthServiceUnitTests()
        {
            _mockUserManager = new Mock<UserManager<AppUser>>(new Mock<IUserStore<AppUser>>().Object, null, null, null, null, null, null, null, null);
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null);
            _mockTokenService = new Mock<ITokenService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockPhotoService = new Mock<IPhotoService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockApplicationDbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            _jwtSettings = Options.Create(new JwtSettings { secretKey = "test_secret", Issuer = "test_issuer", Audience = "test_audience" });

            _authService = new AuthService(_jwtSettings, _mockUserManager.Object, _mockPhotoService.Object, null, _mockRoleManager.Object, _mockHttpContextAccessor.Object, _mockTokenService.Object, _mockApplicationDbContext.Object, _mockEmailService.Object);
        }
        [Fact]
        public async Task RegisterUser_ShouldReturnSuccess_WhenUserIsCreated()
        {
            var registerDto = new RegisterDto { UserName = "testuser", Email = "test@example.com", Password = "Test123!", PhoneNumber = "1234567890" };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);
            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var result = await _authService.RegisterUser(registerDto);

            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            var loginDto = new LoginDto { UserNameOrGmail = "test@example.com", Password = "Test123!" };
            var user = new AppUser { Id = "1", Email = "test@example.com", UserName = "testuser", IsEmailVerificationCodeValid = true };

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockTokenService.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AppUser>(), It.IsAny<IList<string>>())).Returns("mocked_token");

            var result = await _authService.Login(loginDto);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data.Token);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnSuccess_WhenTokenIsValid()
        {
            var validToken = "valid_token";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _mockTokenService.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(principal);
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { Id = "1" });

            var result = await _authService.ValidateToken("Bearer " + validToken);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnSuccess_WhenRefreshTokenIsValid()
        {
            var refreshToken = "valid_refresh_token";
            var user = new AppUser { Id = "1", UserName = "testuser", Email = "test@example.com" };

            _mockHttpContextAccessor.Setup(x => x.HttpContext.Request.Cookies["refreshToken"]).Returns(refreshToken);
            var mockRefreshTokens = new List<RefreshToken>
{
    new RefreshToken { Token = refreshToken, AppUser = user, IsActive = true }
}.AsQueryable();

            var mockDbSet = new Mock<DbSet<RefreshToken>>();
            mockDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.Provider).Returns(mockRefreshTokens.Provider);
            mockDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.Expression).Returns(mockRefreshTokens.Expression);
            mockDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.ElementType).Returns(mockRefreshTokens.ElementType);
            mockDbSet.As<IQueryable<RefreshToken>>().Setup(m => m.GetEnumerator()).Returns(mockRefreshTokens.GetEnumerator());

            _mockApplicationDbContext.Setup(x => x.refreshTokens).Returns(mockDbSet.Object);
            _mockTokenService.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AppUser>(), It.IsAny<IList<string>>())).Returns("new_mocked_token");

            var result = await _authService.RefreshToken();

            Assert.True(result.IsSuccess);
        }
    }
}
