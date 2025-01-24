using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.Register
{
    public class RegisterAppUserHandler : IRequestHandler<RegisterAppUserCommand, Result<Task>>
    {
        private readonly IAuthService _authService;

        public RegisterAppUserHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<Task>> Handle(RegisterAppUserCommand request, CancellationToken cancellationToken)
        {
          return await _authService.RegisterUser(request.RegisterDto);
        }
    }
}
