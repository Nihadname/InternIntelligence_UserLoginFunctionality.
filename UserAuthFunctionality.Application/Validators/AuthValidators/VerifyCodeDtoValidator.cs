using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;

namespace UserAuthFunctionality.Application.Validators.AuthValidators
{
    public class VerifyCodeDtoValidator : AbstractValidator<VerifyCodeDto>
    {
        public VerifyCodeDtoValidator()
        {
            RuleFor(s => s.Email).NotEmpty().EmailAddress().MaximumLength(200).WithMessage("max is 200");
            RuleFor(s => s.Code).NotEmpty().MaximumLength(6);
        }
    }
}
