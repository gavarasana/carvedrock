using CarvedRock.Core;
using CarvedRock.Core.Models;
using CarvedRock.Data.Repositories;
using FluentValidation;

namespace CarvedRock.Domain.Validators
{
    public class NewProductValidator : AbstractValidator<NewProductModel>
    {
        private readonly ICarvedRockRepository _carvedRockRepository;

        internal record PriceRange(double minPrice, double maxPrice);

        internal Dictionary<string, PriceRange> PriceRanges = new()
        {
            {"boots", new PriceRange(50,300) },
            {"kayak", new PriceRange(100,500) },
            {"equip", new PriceRange(20,150) }
        };

        public NewProductValidator(ICarvedRockRepository carvedRockRepository)
        {
            _carvedRockRepository = carvedRockRepository;

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull().WithMessage("{PropertyName} is required.")
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.")
                .MustAsync(NameIsUnique).WithMessage("A product with the same name already exists.");

            RuleFor(p => p.Description)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull().WithMessage("{PropertyName} is required.")
                .MaximumLength(150).WithMessage("{PropertyName} must not exceed 150 characters.");

            RuleFor(p => p.Category)
                .Must(Constants.Categories.Contains)
                .WithMessage($"Category must be one of {string.Join(",",Constants.Categories)}.");

            RuleFor(p => p.Price)
                .Must(PriceIsValid)
                .WithMessage(p => $"Price for {p.Category} must be between {PriceRanges[p.Category]!.minPrice:C} and {PriceRanges[p.Category]!.maxPrice:C}.");

            RuleFor(p => p.ImageUrl)
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute)).WithMessage("{PropertyName} must be a valid URL.")
                .MaximumLength(255).WithMessage("{PropertyName} must not exceed 255 characters.");

        }

        private bool PriceIsValid(NewProductModel newProduct , double priceToValidate)
        {
            if (!PriceRanges.ContainsKey(newProduct.Category)) return true; //Return true if category not found.
            var priceRange = PriceRanges[newProduct.Category];
            return priceToValidate >= priceRange.minPrice && priceToValidate <= priceRange.maxPrice;

        }

        private async Task<bool> NameIsUnique(string productName, CancellationToken token)
        {
            return await _carvedRockRepository.IsProductNameUniqueAsync(productName,token);
        }
    }
}
