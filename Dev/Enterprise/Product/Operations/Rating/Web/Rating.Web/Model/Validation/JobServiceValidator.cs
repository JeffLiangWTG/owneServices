using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Validator class to validate JobService.
	/// </summary>
	public class JobServiceValidator : BaseValidator<JobService>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public JobServiceValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(js => js.Type)
			.NotEmpty()
			.WithMessage(Res.GetString("14A00FEC-9791-4488-BE92-8D2827152FAE", "{0} is Mandatory.", nameof(JobService.Type)));

			var validLocationTypes = new List<string>()
			{
				Location.Types.Country,
				Location.Types.UNLOCO,
			};

			RuleFor(js => js.Location)
			.SetValidator(new LocationValidator(Source, validLocationTypes))
			.When(js => js.Location != null);

			RuleFor(js => js.MeasurementBasis)
			.Must(b => JobService.MeasurementBases.All.Contains(b))
			.WithMessage
				(
					Res.GetString
						(
							"4ECE246C-8D27-4C9A-8F5D-8060CBB75EB0",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(JobService.MeasurementBasis),
							string.Join(", ", JobService.MeasurementBases.All.Select(t => $"'{t}'"))
						)
				)
			.When(js => !string.IsNullOrEmpty(js.MeasurementBasis));

			RuleFor(js => js.MeasurementBasis)
			.Must(b => !string.IsNullOrEmpty(b))
			.WithMessage
				(
					Res.GetString
						(
							"42A6DA6C-6FD7-4E02-A3B4-C12B6FD1D5A9",
							"{0} should be provided when {1} is specified.",
							nameof(JobService.MeasurementBasis),
							nameof(JobService.Rate)
						)
				)
			.When(js => js.Rate > 0);
		}
	}
}
