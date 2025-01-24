using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Core.Entities.Common;

namespace UserAuthFunctionality.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<Task>> RegisterUser(RegisterDto registerDto);
        Task<Result<AuthLoginResponseDto>> Login(LoginDto loginDto);
        Task<Result<bool>> ValidateToken([FromHeader] string Authorization);
    }
}
