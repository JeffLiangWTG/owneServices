using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Validator class to validate Organisation.
	/// </summary>
	public class OrganisationRoleValidator : BaseValidator<OrganisationRole>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public OrganisationRoleValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(d => d.Role)
			.NotEmpty()
			.WithMessage(Res.GetString("318E4F32-8357-42D8-8526-B6A70D339B46", "{0} is Mandatory.", nameof(OrganisationRole.Role)));

			RuleFor(d => d.Code)
			.NotEmpty()
			.WithMessage(Res.GetString("1A54342E-0A35-4757-B047-23BD3A2D525E", "{0} is Mandatory.", nameof(OrganisationRole.Code)));

			RuleFor(d => d.Role)
			.Must(d => OrganisationRole.Roles.All.Contains(d))
			.WithMessage
				(
					Res.GetString
						(
							"78ED172D-0F4D-435C-A423-E7EC9398A484",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(OrganisationRole.Role),
							string.Join(", ", OrganisationRole.Roles.All.Select(t => $"'{t}'"))
						)
				)
			.When(d => !string.IsNullOrEmpty(d.Role));
		}
	}
}
