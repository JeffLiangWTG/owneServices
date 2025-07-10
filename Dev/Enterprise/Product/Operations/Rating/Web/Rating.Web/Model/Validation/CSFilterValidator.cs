using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// CSFilterValidator
	/// </summary>
	public class CSFilterValidator : BaseValidator<CSFilter>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public CSFilterValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(f => f.RateTypes)
			.Must(rts => !rts.Any(rt => string.IsNullOrEmpty(rt)))
			.WithMessage
				(
					Res.GetString
						(
							"79B9ACA4-DF79-481F-84AB-4549EC551C9D",
							"Provided {0} is not valid. It shouldn't contain null or empty elements.",
							nameof(CSFilter.RateTypes)
						)
				)
			.When(f => f.RateTypes?.Any() ?? false);

			RuleFor(f => f.RateTypes2)
			.Must(rts2 => !rts2.Any(rt => string.IsNullOrEmpty(rt)))
			.WithMessage
				(
					Res.GetString
						(
							"B690B70F-9D0E-46FA-AEC8-8504F94ABD59",
							"Provided {0} is not valid. It shouldn't contain null or empty elements.",
							nameof(CSFilter.RateTypes2)
						)
				)
			.When(f => f.RateTypes2?.Any() ?? false);

			RuleFor(f => f.ServiceStrings)
			.Must(ssts => !ssts.Any(sst => string.IsNullOrEmpty(sst)))
			.WithMessage
				(
					Res.GetString
						(
							"A031E83F-0CDC-4443-8931-E6DCB273BCF0",
							"Provided {0} is not valid. It shouldn't contain null or empty elements.",
							nameof(CSFilter.ServiceStrings)
						)
				)
			.When(f => f.ServiceStrings?.Any() ?? false);
		}
	}
}
