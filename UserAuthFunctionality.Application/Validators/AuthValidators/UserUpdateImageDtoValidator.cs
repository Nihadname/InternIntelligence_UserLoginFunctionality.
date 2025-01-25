using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;

namespace UserAuthFunctionality.Application.Validators.AuthValidators
{
    public class UserUpdateImageDtoValidator : AbstractValidator<UserUpdateImageDto>
    {
        public UserUpdateImageDtoValidator()
        {
            RuleFor(s => s).Custom((c, context) =>
            {
                long maxSizeInBytes = 115 * 1024 * 1024;
                if (c.Image == null || !c.Image.ContentType.Contains("image/"))
                {
                    context.AddFailure("Image", "Only image files are accepted");
                }

                if (c.Image != null && c.Image.Length > maxSizeInBytes)
                {
                    context.AddFailure("Image", "Data storage exceeds the maximum allowed size of 15 MB");
                }

            });
        }
    }
}
