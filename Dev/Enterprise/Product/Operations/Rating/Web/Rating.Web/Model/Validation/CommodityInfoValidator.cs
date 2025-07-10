using System.Linq;
using Enterprise.MasterFiles.Business;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// CommodityInfoValidator
	/// </summary>
	public class CommodityInfoValidator : BaseValidator<CommodityInfo>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public CommodityInfoValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(ct => ct.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("87FD7B30-A246-4D9F-AFAC-AE2F5D6A18DD", "{0} is Mandatory.", nameof(CommodityInfo.Type)));

			RuleFor(ct => ct.Type)
			.Must(type => CommodityInfo.Types.All.Contains(type))
			.WithMessage
				(
					Res.GetString
						(
							"EDC5E757-AE69-4A46-9B87-63867C31C2A3",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(CommodityInfo.Type),
							string.Join(", ", CommodityInfo.Types.All.Select(t => $"'{t}'"))
						)
				)
			.When(t => !string.IsNullOrEmpty(t.Type));

			RuleFor(ct => ct.Value)
			.MaximumLength(RefCommodityCode.Schema.RH_CodeMaxLength)
			.WithMessage(Res.GetString("D629F21B-134B-48A8-9950-10E01481BE15", "Provided commodity code ('{{PropertyValue}}') is not valid. It can be a string of maximum {0} characters.", RefCommodityCode.Schema.RH_CodeMaxLength))
			.When(ct => ct.Type == CommodityInfo.Types.CargoWise);

			RuleFor(ct => ct.Value)
			.MaximumLength(RefCommodityCode.Schema.RH_UniversalCommodityGroupMaxLength)
			.WithMessage(Res.GetString("DCC3B813-6A23-4C33-85FF-B30503B4740D", "Provided universal commodity group ('{{PropertyValue}}') is not valid. It can be a string of maximum {0} characters.", RefCommodityCode.Schema.RH_UniversalCommodityGroupMaxLength))
			.When(ct => ct.Type == CommodityInfo.Types.UniversalCommodityGroup);

			var validSourcesForUCG = new[] { SourceEndpoint.Costing, SourceEndpoint.JobCharges };

			if (!validSourcesForUCG.Contains(Source))
			{
				RuleFor(ct => ct.Type)
				.Must(type => CommodityInfo.Types.CargoWise.Equals(type, System.StringComparison.InvariantCultureIgnoreCase))
				.WithMessage
					(
						Res.GetString
							(
								"0DCD2248-7547-4BAC-9454-5A9ADD771A6E",
								"Provided Type ('UCG') is not valid. It can only be used for querying Costings or calculating charges."
							)
					)
				.When(ct => CommodityInfo.Types.UniversalCommodityGroup.Equals(ct.Type, System.StringComparison.InvariantCultureIgnoreCase));
			}
		}
	}
}
