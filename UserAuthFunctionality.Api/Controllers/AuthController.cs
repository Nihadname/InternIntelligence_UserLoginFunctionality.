using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserAuthFunctionality.Application.Features.Auth.Commands.Login;
using UserAuthFunctionality.Application.Features.Auth.Commands.RefreshToken;
using UserAuthFunctionality.Application.Features.Auth.Commands.Register;
using UserAuthFunctionality.Application.Features.Auth.Commands.SendVerificationCode;
using UserAuthFunctionality.Application.Features.Auth.Commands.UpdateImage;
using UserAuthFunctionality.Application.Features.Auth.Commands.VerifyCode;
using UserAuthFunctionality.Application.Features.Auth.Queries;

namespace UserAuthFunctionality.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AuthController : ControllerBase
    { 
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromForm]RegisterAppUserCommand command)
        {
            var result=await _mediator.Send(command);
            return Ok(result);
        }
        [HttpPost("Login")]

        public async Task<IActionResult> Login(LoginAppUserCommand loginAppUserCommand)
        {
            var result = await _mediator.Send(loginAppUserCommand);
            return Ok(result);
        }
        [HttpGet("ValidateToken")]
        [Authorize]
        public async Task<IActionResult> ValidateToken([FromHeader(Name = "Authorization")] ValidateTokenCommand validateTokenCommand)
        {
            var result = await _mediator.Send(validateTokenCommand);
            return Ok(result);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand refreshTokenCommand)
        {
            var result = await _mediator.Send(refreshTokenCommand);
            return Ok(result);
        }
        [HttpPut("UpdateImage")]
        [Authorize]
        public async Task<IActionResult> UpdateImage([FromQuery]UpdateImageCommand updateImageCommand)
        {
            var result = await _mediator.Send(updateImageCommand);
            return Ok(result);
        }
        [HttpGet("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromQuery] VerifyCodeCommand verifyCodeCommand)
        {
            var result = await _mediator.Send(verifyCodeCommand);
            return Ok(result);

        }
        [HttpGet("SendVerificationCode")]

        public async Task<IActionResult> SendVerificationCode([FromQuery] SendVerificationCodeCommand sendVerificationCodeCommand)
        {
            var result = await _mediator.Send(sendVerificationCodeCommand);
            return Ok(result);
        }
        [HttpGet("Profile")]
        [Authorize]
        public async Task<IActionResult> Profile([FromQuery]ProfileCommand profileCommand)
        {
            var result = await _mediator.Send(profileCommand);
            return Ok(result);
        }
    }
}
