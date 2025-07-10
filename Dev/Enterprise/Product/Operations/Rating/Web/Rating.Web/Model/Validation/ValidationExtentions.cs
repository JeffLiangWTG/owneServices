using System.Globalization;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// ValidationExtensions
	/// </summary>
	public static class ValidationExtensions
	{
		/// <summary>
		/// AddErrorsToModelState
		/// </summary>
		/// <param name="result"></param>
		/// <param name="modelState"></param>
		public static void AddErrorsToModelState(this ValidationException validationException, ModelStateDictionary modelState)
		{
			foreach (var error in validationException.Errors)
			{
				var key = error.PropertyName;

				if (modelState.ContainsKey(key))
				{
					modelState[key].Errors.Add(error.ErrorMessage);
				}
				else
				{
					modelState.AddModelError(key, error.ErrorMessage);
					modelState.SetModelValue(key, new ValueProviderResult(error.AttemptedValue ?? "", (error.AttemptedValue ?? "").ToString(), CultureInfo.InvariantCulture));
				}
			}
		}
	}
}
