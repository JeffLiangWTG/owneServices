using System.Collections;
using System.Linq;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public sealed class IsEditableActionFilterAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			base.OnActionExecuted(context);
			if (context != null)
			{
				var repoProvider = context.Controller as IUserAuthorizationHelperProvider;
				var responseContent = context.Result as ObjectResult;
				var results = responseContent?.Value as IEnumerable;
				if (repoProvider != null && results != null)
				{
					var elementType = results.GetType().GetGenericArguments()?.FirstOrDefault();
					if (elementType != null)
					{
						var userId = context.HttpContext.GetUserId();
						responseContent.Value = typeof(ISelectExpandWrapper).IsAssignableFrom(elementType)
							? repoProvider.UserAuthorizationHelper.InvokeGenericMethod(nameof(IAuthorizationHelper.InitAuthorizationsForSelectExpandWrapper), elementType, results, userId)
							: repoProvider.UserAuthorizationHelper.InvokeGenericMethod(nameof(IAuthorizationHelper.InitAuthorizations), elementType, results, userId);
					}
				}
			}
		}
	}
}
