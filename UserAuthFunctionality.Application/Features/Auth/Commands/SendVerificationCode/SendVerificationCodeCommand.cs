using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Commands.SendVerificationCode
{
  
    public record SendVerificationCodeCommand(string email):IRequest<Result<string>>;   
}
