﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Application.Dtos.Auth
{
    public class UserGetDto
    {
        public string FullName { get; init; }
        public string Id { get; init; }
        public string UserName { get; init; }
        public string PhoneNumber { get; init; }
        public string Email { get; init; }
        public string Image { get; init; }
        public bool IsBlocked { get; init; }
    }
}
