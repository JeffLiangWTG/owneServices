using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// Base valication class to validate RateQuery as an API input.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BaseValidator<T> : AbstractValidator<T>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public BaseValidator(SourceEndpoint source)
		{
			this.Source = source;
		}

		/// <summary>
		/// Source Endpoint which triggers the validaiton.
		/// </summary>
		protected SourceEndpoint Source { get; }
	}
}
