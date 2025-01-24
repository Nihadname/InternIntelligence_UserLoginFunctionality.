using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Application.Dtos.Auth
{
    public class LoginDto
    {
        public string UserNameOrGmail { get; init; }
        public string Password { get; init; }
    }
}
