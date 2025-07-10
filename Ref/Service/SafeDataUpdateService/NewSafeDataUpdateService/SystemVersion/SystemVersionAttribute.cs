using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public sealed class SystemVersionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);
			var query = context.HttpContext?.Request?.GetEncodedPathAndQuery();
			if (query != null)
			{
				var queryParams = HttpUtility.ParseQueryString(query);
				var systemVersion = queryParams["SystemVersionUTC"];
				var systemVersionContext = context.HttpContext.RequestServices.GetRequiredService<ISystemVersionContext>();
				systemVersionContext.SystemVersionUTC = systemVersion;
			}
		}
	}
}
