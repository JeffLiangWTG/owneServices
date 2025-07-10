using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// CGFilterValidator
	/// </summary>
	public class CGFilterValidator : BaseValidator<CGFilter>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public CGFilterValidator(SourceEndpoint source) : base(source)
		{
			RuleFor(f => f.Products)
			.Must(ps => !ps.Any(p => string.IsNullOrEmpty(p)))
			.WithMessage
				(
					Res.GetString
						(
							"647D3D82-B718-40F8-9B8B-2D028AD418EE",
							"Provided {0} is not valid. It shouldn't contain null or empty elements.",
							nameof(CGFilter.Products)
						)
				)
			.When(f => f.Products?.Any() ?? false);

			RuleFor(f => f.References)
			.Must(rs => !rs.Any(r => string.IsNullOrEmpty(r)))
			.WithMessage
				(
					Res.GetString
						(
							"29CCEB52-464C-4F7D-B214-596934D8369D",
							"Provided {0} is not valid. It shouldn't contain null or empty elements.",
							nameof(CGFilter.References)
						)
				)
			.When(f => f.References?.Any() ?? false);

			RuleFor(f => f.Vias)
			.Must(vs => !vs.Any(v => string.IsNullOrEmpty(v)))
			.WithMessage
				(
					Res.GetString
						(
							"12485D04-0E27-4D55-9F31-70507F87FC77",
							"Provided {0} is not valid. It shouldn't contain null or empty elements.",
							nameof(CGFilter.Vias)
						)
				)
			.When(f => f.Vias?.Any() ?? false);
		}
	}
}
