using CargoWise.RefDbRepo.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common.Web.ExceptionHandler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class ApiExceptionHandlerExtensions
	{
		public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app)
		{
			return app.UseCommonApiExceptionHandler(true, (context, apiException) =>
			{
				var errorReportWrapper = context.RequestServices.GetRequiredService<ErrorReportingClientWrapper>();
				if (context.ShouldReportIssue())
				{
					errorReportWrapper.PostCrashReport(apiException.Exception, "Safe Update Service", null);
				}
				else
				{
					apiException.RequestUri = "[This exception will be handled by another program.]" + apiException.RequestUri;
				}
			});
		}
	}
}
