using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Application.Interfaces;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Queries
{
    public class ProfileHandler : IRequestHandler<ProfileCommand, Result<UserGetDto>>
    {
        private readonly IAuthService _authService;

        public ProfileHandler(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<Result<UserGetDto>> Handle(ProfileCommand request, CancellationToken cancellationToken)
        {
            return await _authService.Profile();
        }
    }
}
