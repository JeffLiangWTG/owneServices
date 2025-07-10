using System.Linq;
using Enterprise.MasterFiles.Business;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Validator class to validate JobContainer.
	/// </summary>
	public class JobContainerValidator : BaseValidator<JobContainer>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		/// <param name="isContainerized"></param>
		public JobContainerValidator(SourceEndpoint source, bool isContainerized) : base(source)
		{
			if (source == SourceEndpoint.JobCharges && isContainerized)
			{
				RuleFor(jc => jc.ContainerTypeCWCode)
				.NotEmpty()
				.WithMessage(Res.GetString("24B15C9C-CBD6-4E7D-9538-C8026928981E", "When job is containerized, {0} is Mandatory.", nameof(JobContainer.ContainerTypeCWCode)));

				RuleFor(jc => jc.Unit)
				.NotEmpty()
				.WithMessage(Res.GetString("75BC2722-F04D-426E-81C8-47CA116C11FF", "When job is containerized, {0} cannot be null.", nameof(JobContainer.Unit)));
			}

			RuleFor(ct => ct.ContainerTypeCWCode)
			.MaximumLength(RefContainer.Schema.RC_CodeMaxLength)
			.WithMessage(Res.GetString("9548B352-4D50-48BA-A469-5D301D30350D", "Provided code ('{{PropertyValue}}') is not valid. It can be a string of maximum {0} characters.", RefContainer.Schema.RC_CodeMaxLength));

			RuleFor(ct => ct.Ownership)
			.Must(o => JobContainer.Ownerships.All.Contains(o.ToUpperInvariant()))
			.WithMessage(Res.GetString
						(
							"AC62EFA5-2D24-4DBC-9C7A-E0AA8316F787",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(JobContainer.Ownership),
							string.Join(", ", JobContainer.Ownerships.All.Select(t => $"'{t}'"))
						))
			.When(ct => !string.IsNullOrEmpty(ct.Ownership));

			RuleFor(jc => jc.PackLines)
				.ForEach(p => p.SetValidator(new JobPackLineValidator(Source)))
				.When(jc => jc.PackLines?.Any() ?? false);
		}
	}
}
