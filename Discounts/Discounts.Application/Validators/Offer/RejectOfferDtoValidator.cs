using Discounts.Application.DTOs.Admin;
using FluentValidation;

namespace Discounts.Application.Validators.Admin
{
    public class RejectOfferDtoValidator : AbstractValidator<RejectOfferDto>
    {
        public RejectOfferDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MinimumLength(5).WithMessage("Reason must be at least 5 characters long");
        }
    }
}
