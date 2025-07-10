using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public class CustomAuthorizationMiddleware
	{
		readonly string responseMessage = "{ \"_err\":\"You do not have access to execute or modify the job.\"}";
		readonly RequestDelegate next;
		readonly IAuthorizationHelper authorizationHelper;

		public CustomAuthorizationMiddleware(RequestDelegate next, IAuthorizationHelper authorizationHelper)
		{
			this.next = next;
			this.authorizationHelper = authorizationHelper;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var isAllowed = false;
			if (context.Request.HasFormContentType)
			{
				var formCollectionService = context.RequestServices.GetRequiredService<IFormCollectionService>();
				var command = formCollectionService.Command;
				if (string.IsNullOrEmpty(command) || formCollectionService.IsGetCommand)
				{
					isAllowed = true;
				}
				else
				{
					var userName = context.User?.Identity?.Name;
					if (!string.IsNullOrEmpty(userName) && authorizationHelper.IsAuthorized(userName, formCollectionService))
					{
						isAllowed = true;
					}
				}
			}
			else
			{
				isAllowed = true;
			}

			if (isAllowed)
			{
				await next(context);
			}
			else
			{
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(responseMessage);
				return;
			}
		}
	}

	public static class CustomAuthorizationAppBuilderExtensions
	{
		public static IApplicationBuilder UseCustomAuthorization(this IApplicationBuilder app, IAuthorizationHelper authorizationHelper)
		{
			return app.UseMiddleware<CustomAuthorizationMiddleware>(authorizationHelper);
		}
	}
}
