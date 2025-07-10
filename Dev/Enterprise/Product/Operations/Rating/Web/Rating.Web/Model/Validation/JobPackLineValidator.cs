using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Validator class to validate JobPackLine.
	/// </summary>
	public class JobPackLineValidator : BaseValidator<JobPackLine>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public JobPackLineValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(p => p.WeightUnit)
			.NotEmpty()
			.WithMessage(Res.GetString("277D5829-B5BE-403B-B5E9-7CDA4BD21E3E", "{0} is mandatory when {1} is specified.", nameof(JobPackLine.WeightUnit), nameof(JobPackLine.Weight)))
			.When(p => p.Weight.HasValue);

			RuleFor(p => p.WeightUnit)
			.Must(u => Core.Constants.Weight.Codes.Contains(u.ToUpperInvariant()))
			.WithMessage(Res.GetString
						(
							"AC62EFA5-2D24-4DBC-9C7A-E0AA8316F787",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(JobPackLine.WeightUnit),
							string.Join(", ", Core.Constants.Weight.Codes.Select(t => $"'{t}'"))
						))
			.When(p => !string.IsNullOrEmpty(p.WeightUnit));

			RuleFor(p => p.VolumeUnit)
			.NotEmpty()
			.WithMessage(Res.GetString("542E7885-3ADB-4A82-8BEB-C7323BACDA80", "{0} is mandatory when {1} is specified.", nameof(JobPackLine.VolumeUnit), nameof(JobPackLine.Volume)))
			.When(p => p.Volume.HasValue);

			RuleFor(p => p.VolumeUnit)
			.Must(u => Core.Constants.Volume.Codes.Contains(u.ToUpperInvariant()))
			.WithMessage(Res.GetString
						(
							"E7B94532-E2A2-42DA-88D1-AC3D33427E7B",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(JobPackLine.VolumeUnit),
							string.Join(", ", Core.Constants.Volume.Codes.Select(t => $"'{t}'"))
						))
			.When(p => !string.IsNullOrEmpty(p.VolumeUnit));

			RuleFor(p => p.DGClass)
			.Empty()
			.WithMessage(Res.GetString("B5129C3A-B502-44FC-A069-A5BF899599A5", "You should set either {0} or {1}.", nameof(JobPackLine.DGClass), nameof(JobPackLine.DGSubstance)))
			.When(p => !string.IsNullOrEmpty(p.DGSubstance));
		}
	}
}
