using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Application.Dtos.Auth
{
    public class AuthLoginResponseDto
    {
        public bool IsLogined { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
