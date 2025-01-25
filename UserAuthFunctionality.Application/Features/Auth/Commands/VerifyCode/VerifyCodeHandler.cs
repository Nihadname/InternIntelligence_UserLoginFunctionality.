using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.VerifyCode
{
    public class VerifyCodeHandler : IRequestHandler<VerifyCodeCommand, Result<string>>
    {
        private readonly IAuthService _authService;

        public VerifyCodeHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<string>> Handle(VerifyCodeCommand request, CancellationToken cancellationToken)
        {
            return await _authService.VerifyCode(request.VerifyCodeDto);
        }
    }
}
