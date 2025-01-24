using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Features.Auth.Queries
{
  
    public record ValidateTokenCommand(string Authorization) : IRequest<Result<bool>>;

}
