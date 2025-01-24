using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.RefreshToken
{
    
    public record RefreshTokenCommand() : IRequest<Result<AuthLoginResponseDto>>;

}
