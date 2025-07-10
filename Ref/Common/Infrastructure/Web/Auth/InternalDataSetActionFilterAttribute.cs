using System;
using System.Collections;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.Common.TypeProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class InternalDataSetActionFilterAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var userIdentity = context.HttpContext.User.Identity;
			if (userIdentity == null || !userIdentity.IsAuthenticated)
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			var userId = context.HttpContext.GetUserId();
			if (string.IsNullOrEmpty(userId))
			{
				context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
			}
		}

		public override void OnResultExecuting(ResultExecutingContext context)
		{
			if (context.Result is ObjectResult responseContent && responseContent.Value is IEnumerable results)
			{
				var responseDataType = results.GetType().GetGenericArguments().FirstOrDefault();
		
				if (responseDataType != null)
				{
					var userId = context.HttpContext.GetUserId();
					var authorizationHelper = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationHelper>();

					var authorizedResults = (IEnumerable)authorizationHelper.InvokeGenericMethod(
						nameof(IAuthorizationHelper.FilterAuthorizedData),
						responseDataType, results, userId);

					var authResultsCount = authorizedResults.Cast<object>().Count();
					var resultsCount = results.Cast<object>().Count();
					if (authResultsCount == 0 && resultsCount != 0)
					{
						context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
						return;
					}
		
					responseContent.Value = authorizedResults;
				}
			}
		}
	}
}
