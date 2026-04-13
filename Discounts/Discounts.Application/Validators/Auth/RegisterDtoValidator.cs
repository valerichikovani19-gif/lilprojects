using Discounts.Application.DTOs.Auth;
using FluentValidation;

namespace Discounts.Application.Validators.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters ");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Firstname is required")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Lastname is required")
                .MaximumLength(50);

            RuleFor(x => x.Role)
                .Must(role => role == "Merchant" || role == "Customer")
                .WithMessage("Role must be 'Merchant' or 'Customer'");
        }
    }
}
