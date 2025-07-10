using System.Linq;
using Enterprise.MasterFiles.Business;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// ContainerTypeValidator
	/// </summary>
	public class ContainerTypeValidator : BaseValidator<ContainerType>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public ContainerTypeValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(ct => ct.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("6C2305D0-2B44-418E-A557-729F6B5D4440", "{0} is Mandatory.", nameof(ContainerType.Type)));

			RuleFor(ct => ct.Type)
			.Must(type => ContainerType.Types.All.Contains(type))
			.WithMessage
				(
					Res.GetString
						(
							"521952DA-E2FE-47C1-83A2-E27C60E5EED7",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(ContainerType.Type),
							string.Join(", ", ContainerType.Types.All.Select(t => $"'{t}'"))
						)
				)
			.When(t => !string.IsNullOrEmpty(t.Type));

			RuleFor(ct => ct.Value)
			.NotEmpty()
			.WithMessage(Res.GetString("FB99FE38-869C-48E5-B953-E8A39616C338", "{0} is Mandatory.", nameof(ContainerType.Value)));

			RuleFor(ct => ct.Value)
			.MaximumLength(RefContainer.Schema.RC_CodeMaxLength)
			.WithMessage(Res.GetString("A8108CBF-F169-4A2F-92F0-58A76215FDA6", "Provided CargoWise code ('{{PropertyValue}}') is not valid. It can be a string of maximum {0} characters.", RefContainer.Schema.RC_CodeMaxLength))
			.When(ct => ct.Type == ContainerType.Types.CargoWise);

			RuleFor(ct => ct.Value)
			.MaximumLength(RefContainer.Schema.RC_ISOTypeMaxLength)
			.WithMessage(Res.GetString("553C2440-C307-499B-9825-6C6E24AEE16E", "Provided ISO code ('{{PropertyValue}}') is not valid. It can be a string of maximum {0} characters.", RefContainer.Schema.RC_ISOTypeMaxLength))
			.When(ct => ct.Type == ContainerType.Types.ISO);
		}
	}
}
