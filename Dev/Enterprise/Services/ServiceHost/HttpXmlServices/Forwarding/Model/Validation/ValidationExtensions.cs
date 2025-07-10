using System.Globalization;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using FluentValidation.Results;

namespace Enterprise.Services.ServiceHost
{
	public static class ValidationExtensions
	{
		public static void AddErrorsToModelState(this ValidationResult result, ModelStateDictionary modelState)
		{
			if (!result.IsValid)
			{
				foreach (var error in result.Errors)
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
}
