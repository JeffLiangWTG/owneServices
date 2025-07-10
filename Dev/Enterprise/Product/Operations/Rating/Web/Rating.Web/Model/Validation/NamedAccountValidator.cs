using System.Linq;
using Enterprise.MasterFiles.Business;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// NamedAccountValidator
	/// </summary>
	public class NamedAccountValidator : BaseValidator<NamedAccount>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public NamedAccountValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(csl => csl.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("66459990-E478-4687-A98D-2C53D1935AE5", "{0} is Mandatory.", nameof(NamedAccount.Type)));

			RuleFor(csl => csl.Type)
			.Must(type => NamedAccount.Types.All.Contains(type))
			.WithMessage
				(
					Res.GetString
						(
							"E3B0750A-113F-4309-A2F4-B838C89672A7",
							"Provided Type ('{{PropertyValue}}') is not valid. It can only be one of these values: {0}.",
							string.Join(", ", NamedAccount.Types.All.Select(t => $"'{t}'"))
						)
				)
			.When(t => !string.IsNullOrEmpty(t.Type));

			RuleFor(csl => csl.Value)
			.NotEmpty()
			.WithMessage(Res.GetString("0337A640-41DC-48C5-AA41-0C16F4A22A4A", "{0} is Mandatory.", nameof(NamedAccount.Value)));

			RuleFor(na => na.Value)
			.MaximumLength(OrgHeader.Schema.OH_CodeMaxLength)
			.WithMessage
				(
					Res.GetString
						(
							"3F4A8EC4-A6A3-486A-B09D-F3EA8F91683E",
							"Provided named account ('{{PropertyValue}}') is not valid. It can be a string of maximum {0} characters.",
							OrgHeader.Schema.OH_CodeMaxLength
						)
				)
			.When(na => na.Type == NamedAccount.Types.CargoWise);

			var validSourcesForNAC = new[] { SourceEndpoint.Costing, SourceEndpoint.JobCharges };

			if (!validSourcesForNAC.Contains(Source))
			{
				RuleFor(csl => csl.Type)
				.Must(type => NamedAccount.Types.CargoWise.Equals(type, System.StringComparison.InvariantCultureIgnoreCase))
				.WithMessage
					(
						Res.GetString
							(
								"B69FCC17-5F49-4FF3-A644-DAC4ABE392CD",
								"Provided Type ('NAC') is not valid. It can only be used for querying Costings or calculating charges."
							)
					)
				.When(type => NamedAccount.Types.NamedAccount.Equals(type.Type, System.StringComparison.InvariantCultureIgnoreCase));
			}
		}
	}
}
