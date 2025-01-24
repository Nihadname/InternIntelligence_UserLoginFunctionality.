using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Queries
{
    public class ValidateTokenHandler : IRequestHandler<ValidateTokenCommand, Result<bool>>
    {
        private readonly IAuthService _authService;

        public ValidateTokenHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<bool>> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
        {
            return await _authService.ValidateToken(request.Authorization);
        }
    }
}
