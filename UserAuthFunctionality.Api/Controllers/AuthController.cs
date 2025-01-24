using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserAuthFunctionality.Application.Features.Auth.Commands.Login;
using UserAuthFunctionality.Application.Features.Auth.Commands.RefreshToken;
using UserAuthFunctionality.Application.Features.Auth.Commands.Register;
using UserAuthFunctionality.Application.Features.Auth.Queries;
using UserAuthFunctionality.Application.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace UserAuthFunctionality.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
