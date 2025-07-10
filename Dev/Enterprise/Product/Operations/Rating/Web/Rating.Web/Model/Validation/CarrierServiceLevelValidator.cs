using System.Linq;
using Enterprise.MasterFiles.Business;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// 
	/// </summary>
	public class CarrierServiceLevelValidator : BaseValidator<CarrierServiceLevel>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public CarrierServiceLevelValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(csl => csl.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("84EDA517-764E-4A90-8951-24D07ED7FAA3", "{0} is Mandatory.", nameof(CarrierServiceLevel.Type)));

			RuleFor(csl => csl.Type)
			.Must(type => CarrierServiceLevel.Types.All.Contains(type))
			.WithMessage
				(
					Res.GetString
						(
							"318893B2-B6A4-498B-A7BF-024C77E6C253",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(CarrierServiceLevel.Type),
							string.Join(", ", CarrierServiceLevel.Types.All.Select(t => $"'{t}'"))
						)
				)
			.When(t => !string.IsNullOrEmpty(t.Type));

			RuleFor(csl => csl.Value)
			.MaximumLength(OrgCarrierServiceLevel.Schema.PL_CodeMaxLength)
			.WithMessage
				(
					Res.GetString
						(
							"462AC3DD-3604-4D88-A373-BA500692189F",
							"Provided {0} as CargoWise code ('{{PropertyValue}}') is not valid. It can be a string of maximum {1} characters.",
							nameof(CarrierServiceLevel.Value),
							OrgCarrierServiceLevel.Schema.PL_CodeMaxLength
						)
				)
			.When(csl => csl.Type == CarrierServiceLevel.Types.CargoWise);

			RuleFor(csl => csl.Value)
			.MaximumLength(OrgCarrierServiceLevel.Schema.PL_CarrierServiceCodeMaxLength)
			.WithMessage
				(
					Res.GetString
						(
							"2A4EE990-D9EB-440D-A5F5-B786C6C72AF1",
							"Provided {0} as Universal code ('{{PropertyValue}}') is not valid. It can be a string of maximum {1} characters.",
							nameof(CarrierServiceLevel.Value),
							OrgCarrierServiceLevel.Schema.PL_CarrierServiceCodeMaxLength
						)
				)
			.When(csl => csl.Type == CarrierServiceLevel.Types.Universal);

			var validSourcesForUC = new[] { SourceEndpoint.Costing, SourceEndpoint.JobCharges };

			if (!validSourcesForUC.Contains(Source))
			{
				RuleFor(ct => ct.Type)
				.Must(type => CarrierServiceLevel.Types.CargoWise.Equals(type, System.StringComparison.InvariantCultureIgnoreCase))
				.WithMessage
					(
						Res.GetString
							(
								"FCB18EF1-730A-4DFA-9194-2129120160AC",
								"Provided Type ('UC') is not valid. It can only be used for querying Costings or calculating charges."
							)
					)
				.When(ct => CarrierServiceLevel.Types.Universal.Equals(ct.Type, System.StringComparison.InvariantCultureIgnoreCase));
			}
		}
	}
}
