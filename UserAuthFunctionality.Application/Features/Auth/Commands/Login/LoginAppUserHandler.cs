using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Application.Features.Auth.Commands.Register;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.Login
{
    public class LoginAppUserHandler : IRequestHandler<LoginAppUserCommand, Result<AuthLoginResponseDto>>
    {
        private readonly IAuthService _authService;

        public LoginAppUserHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<AuthLoginResponseDto>> Handle(LoginAppUserCommand request, CancellationToken cancellationToken)
        {
            return await _authService.Login(request.Login);
        }
    }
}
