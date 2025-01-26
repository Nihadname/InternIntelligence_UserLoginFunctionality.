using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.SendVerificationCode
{
    public class SendVerificationCodeHandler : IRequestHandler<SendVerificationCodeCommand, Result<string>>
    {
        private readonly IAuthService _authService;

        public SendVerificationCodeHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<string>> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
        {
            return await _authService.SendVerificationCode(request.email);
        }
    }
}
