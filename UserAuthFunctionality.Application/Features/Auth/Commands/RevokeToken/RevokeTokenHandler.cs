using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.RevokeToken
{
    public class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result<string>>
    {
        private readonly IAuthService _authService;

        public RevokeTokenHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<string>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            return await _authService.RevokeRefreshToken();
        }
    }
}
