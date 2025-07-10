using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Organisation Validator
	/// </summary>
	public class OrganisationValidator : BaseValidator<Organisation>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public OrganisationValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(org => org)
			.Must
				(org =>
					(new string[] { org.CWCode, org.SCAC, org.IATACode, org.C1CCode }).Where(s => !string.IsNullOrEmpty(s)).Count() == 1
				)
			.WithMessage
				(
					Res.GetString
						(
							"93DC21F8-3138-482D-99F5-31B3BB12D950",
							"One and only one of the following properties must be specified for an organization: '{0}', '{1}', '{2}' or '{3}'.",
							nameof(Organisation.CWCode),
							nameof(Organisation.SCAC),
							nameof(Organisation.IATACode),
							nameof(Organisation.C1CCode)
						)
				)
			.When(org => org != null);

			RuleFor(org => org.C1CCode)
			.Must(c1cCode => c1cCode.Trim().Length == 4)
			.WithMessage(
				Res.GetString
					(
						"1a5a9301-aaa6-48b1-bcc2-9f5384e56d45",
						"Provided {0} ('{{PropertyValue}}') is not valid. {1} length must be 4.",
						nameof(Organisation.C1CCode),
						nameof(Organisation.C1CCode)
					)
				)
			.When(org => !string.IsNullOrEmpty(org.C1CCode));

			RuleFor(org => org.SCAC)
			.Must(scacCode => scacCode.Trim().Length == 4)
			.WithMessage(
				Res.GetString
					(
						"96ad7366-9eae-410e-9163-6bb408850ae5",
						"Provided {0} code ('{{PropertyValue}}') is not valid. {1} code length must be 4.",
						nameof(Organisation.SCAC),
						nameof(Organisation.SCAC)
					)
				)
			.When(org => !string.IsNullOrEmpty(org.SCAC));

			RuleFor(org => org.IATACode)
			.Must(iataCode => iataCode.Trim().Length >= 2 && iataCode.Trim().Length <= 4)
			.WithMessage(
			Res.GetString
					(
						"da92e390-8b1e-482b-99c3-35c88e336e9a",
						"Provided {0} ('{{PropertyValue}}') is not valid. {1} length must be from 2 to 4.",
						nameof(Organisation.IATACode),
						nameof(Organisation.IATACode)
					)
				)
			.When(org => !string.IsNullOrEmpty(org.IATACode));
		}
	}
}
