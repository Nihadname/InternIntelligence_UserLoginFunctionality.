﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Application.Dtos.Auth
{
    public class VerifyCodeDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
