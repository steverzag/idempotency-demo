using FluentValidation;
using IdempotencyDemo.API.DTOs;

namespace IdempotencyDemo.API.Validation
{
	public class CreateUserValidator : AbstractValidator<CreateUserRequest>
	{
		public CreateUserValidator() 
		{
			RuleFor(u => u.FirstName)
				.NotEmpty()
				.WithMessage("First name is required")
				.Length(2, 50)
				.WithMessage("First name must be between 2 and 50 characters long");
			RuleFor(u => u.FirstName)
				.NotEmpty()
				.WithMessage("Last name is required")
				.Length(2, 50)
				.WithMessage("Last name must be between 2 and 50 characters long");
			RuleFor(u => u.Email)
				.NotEmpty()
				.WithMessage("Email is required")
				.EmailAddress()
				.WithMessage("Invalid email format");
		}
	}

	public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
	{
		public UpdateUserValidator()
		{
			RuleFor(u => u.FirstName).NotEmpty()
				.WithMessage("First name is required")
				.Length(2, 50)
				.WithMessage("First name must be between 2 and 50 characters long");
			RuleFor(u => u.FirstName).NotEmpty()
				.WithMessage("Last name is required")
				.Length(2, 50)
				.WithMessage("Last name must be between 2 and 50 characters long");
			RuleFor(u => u.Email).NotEmpty()
				.WithMessage("Email is required")
				.EmailAddress()
				.WithMessage("Invalid email format");
		}
	}
}
