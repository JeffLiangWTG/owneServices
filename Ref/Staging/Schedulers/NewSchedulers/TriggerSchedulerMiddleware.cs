using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schedulers.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers
{
	public class TriggerSchedulerMiddleware
	{
		readonly RequestDelegate next;
		readonly ISchedulerValidator schedulerValidator;

		public TriggerSchedulerMiddleware(RequestDelegate next, ISchedulerValidator schedulerValidator)
		{
			this.next = next;
			this.schedulerValidator = schedulerValidator;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (!context.Request.HasFormContentType)
			{
				await next(context);
				return;
			}

			var formCollectionService = context.RequestServices.GetRequiredService<IFormCollectionService>();
			var command = formCollectionService.Command;
			if (string.IsNullOrEmpty(command))
			{
				await next(context);
				return;
			}

			if (!schedulerValidator.Validate(formCollectionService, out var errorMessage))
			{
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(errorMessage);
				return;
			}
			await next(context);
		}

	}

	public static class TriggerJobDataMapMiddlewareExtensions
	{
		public static IApplicationBuilder CheckTriggerScheduler(this IApplicationBuilder app, ISchedulerValidator schedulerValidator)
		{
			return app.UseMiddleware<TriggerSchedulerMiddleware>(schedulerValidator);
		}
	}
}
