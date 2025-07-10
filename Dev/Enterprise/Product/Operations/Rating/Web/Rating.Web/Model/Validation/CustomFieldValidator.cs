using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	public class CustomFieldValidator : BaseValidator<CustomField>
	{
		public CustomFieldValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(d => d.Name)
				.NotEmpty()
				.WithMessage(Res.GetString("9BE5BF16-6137-4AA2-8DF7-E93350478074", "{0} is mandatory", nameof(CustomField.Name)));

			RuleFor(d => d.Value)
				.NotEmpty()
				.WithMessage(Res.GetString("F46C8765-38EA-4C23-9142-2776E020C314", "{0} is mandatory", nameof(CustomField.Value)));
		}
	}
}
