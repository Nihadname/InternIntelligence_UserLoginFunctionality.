using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;

namespace UserAuthFunctionality.Application.Validators.AuthValidators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(s=>s.UserNameOrGmail).NotEmpty().WithMessage("not empty")
                .MaximumLength(150).WithMessage("max is 150");
            RuleFor(s => s.Password).NotEmpty().WithMessage("not empty")
           .MinimumLength(8)
           .MaximumLength(100).WithMessage("max is 100");

        }
    }
}
