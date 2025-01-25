﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAuthFunctionality.Application.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string body, string email, string title, string subject);
    }
}
