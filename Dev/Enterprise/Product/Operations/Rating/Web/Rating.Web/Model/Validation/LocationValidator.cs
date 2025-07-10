using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Business;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Validator class to validate Location.
	/// </summary>
	public class LocationValidator : BaseValidator<Location>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		/// <param name="validLocationTypes"></param>
		public LocationValidator(SourceEndpoint source, IEnumerable<string> validLocationTypes, bool enforceRelatedFieldValidation = false) : base(source)
		{
			DefineTypeMandatoryValidation();

			DefineValueMandatoryValidation();

			if (enforceRelatedFieldValidation)
			{
				DefineRelatedFieldValidation();
			}

			RuleFor(l => l.Type)
			.Must(t => validLocationTypes.Contains(t))
			.WithMessage
				(
					Res.GetString
						(
							"CCEAB5E-9B68-4516-B2CD-98BBB77BC1F0",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(Location.Type),
							string.Join(", ", validLocationTypes.Select(t => $"'{t}'"))
						)
				)
			.When(t => !string.IsNullOrEmpty(t.Type));

			RuleFor(l => l.Value)
			.Must(v => v.Length == 2)
			.WithMessage(Res.GetString("F8188E68-38B9-4E93-89AF-24197693217C", "Provided Country/Region ('{PropertyValue}') is not valid. It should be a string of 2 characters."))
			.When(l => !string.IsNullOrEmpty(l.Value) && l.Type == Location.Types.Country);

			RuleFor(l => l.Value)
			.Must(v => v.Length == 3)
			.WithMessage(Res.GetString("2B62C5E6-CEA3-4DE1-8127-67021565E6F7", "Provided IATA City ('{PropertyValue}') is not valid. It should be a string of 3 characters."))
			.When(l => !string.IsNullOrEmpty(l.Value) && l.Type == Location.Types.IATACity);

			RuleFor(l => l.Value)
			.Must(v => v.Length == 4)
			.WithMessage(Res.GetString("77877FAE-7A1F-4B0D-BB50-26CDA34114BF", "Provided International Zone ('{PropertyValue}') is not valid. It should be a string of 4 characters."))
			.When(l => !string.IsNullOrEmpty(l.Value) && l.Type == Location.Types.Zone);

			RuleFor(l => l.Value)
			.Must(v => v.Length == 5)
			.WithMessage(Res.GetString("7F9B1285-C591-4E1B-8BBE-82B9791AEA85", "Provided UNLOCO ('{PropertyValue}') is not valid. It should be a string of 5 characters."))
			.When(l => !string.IsNullOrEmpty(l.Value) && l.Type == Location.Types.UNLOCO);
		}

		/// <summary>
		/// DefineTypeMandatoryValidation
		/// </summary>
		protected virtual void DefineTypeMandatoryValidation()
		{
			RuleFor(l => l.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("10A1132C-5782-406F-BDD4-F8CDBD8FEFB9", "{0} is Mandatory.", nameof(Location.Type)));
		}

		/// <summary>
		/// DefineValueMandatoryValidation
		/// </summary>
		protected virtual void DefineValueMandatoryValidation()
		{
			RuleFor(l => l.Value)
			.NotEmpty()
			.WithMessage(Res.GetString("D6034ACE-1399-4CEC-A7C0-3C8BF5A403C8", "{0} is Mandatory.", nameof(Location.Value)));
		}

		void DefineRelatedFieldValidation()
		{
			RuleFor(l => l.RelatedField)
			.NotEmpty()
			.WithMessage(Res.GetString("91A207B2-3CDC-4CD2-92BB-4DB30AC25A1C", "{0} is Mandatory.", nameof(Location.RelatedField)));

			var relatedFieldTypes = new[]
			{
				RateEntryLookups.LocationSourceOption.FirstLoad.Code,
				RateEntryLookups.LocationSourceOption.LastDischarge.Code,
				RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code,
				RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code,
			};

			RuleFor(l => l.RelatedField)
			.Must(rf => relatedFieldTypes.Contains(rf))
			.WithMessage(Res.GetString("BDAC95F4-EEE8-49D4-8869-D3C26C4CEC78", "Provided Related Field ('{{PropertyValue}}') is not valid. It can only be one of these values: {0}", string.Join(", ", relatedFieldTypes)));
		}
	}
}
