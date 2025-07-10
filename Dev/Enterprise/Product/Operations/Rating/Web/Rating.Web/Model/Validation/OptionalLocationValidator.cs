using System.Collections.Generic;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// OptionalLocationValidator
	/// </summary>
	public class OptionalLocationValidator : LocationValidator
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		/// <param name="validLocationTypes"></param>
		public OptionalLocationValidator(SourceEndpoint source, IEnumerable<string> validLocationTypes) : base(source, validLocationTypes)
		{
		}

		/// <summary>
		/// DefineTypeMandatoryValidation
		/// </summary>
		protected override void DefineTypeMandatoryValidation()
		{
			RuleFor(l => l.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("2cf0e6b7-60c3-4441-93b2-191a70bf3fb6", "{0} is mandatory when the {1} is specified.", nameof(Location.Type), nameof(Location.Value)))
			.When(q => !string.IsNullOrEmpty(q.Value));
		}

		/// <summary>
		/// DefineValueMandatoryValidation
		/// </summary>
		protected override void DefineValueMandatoryValidation()
		{
			RuleFor(l => l.Value)
			.NotEmpty()
			.WithMessage(Res.GetString("67a48800-a708-40b6-9123-d7a4f41ffc53", "{0} is mandatory when the {1} is specified.", nameof(Location.Value), nameof(Location.Type)))
			.When(q => !string.IsNullOrEmpty(q.Type));
		}
	}
}
