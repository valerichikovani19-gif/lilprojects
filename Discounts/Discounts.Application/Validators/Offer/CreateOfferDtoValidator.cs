using Discounts.Application.DTOs.Offer;
using FluentValidation;

namespace Discounts.Application.Validators.Offer
{
    public class CreateOfferDtoValidator : AbstractValidator<CreateOfferDto>
    {
        public CreateOfferDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.OriginalPrice)
                .GreaterThan(0).WithMessage("Original price must be greater than 0");

            // business logic
            RuleFor(x => x.DiscountPrice)
                .GreaterThan(0)
                .LessThan(x => x.OriginalPrice).WithMessage("Discount price must be less than original price");

            RuleFor(x => x.ValidUntil)
                .GreaterThan(x => x.ValidFrom).WithMessage("End date must be after start date")
                .GreaterThan(DateTime.UtcNow).WithMessage("End date must be in the future");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required");
        }
    }
}