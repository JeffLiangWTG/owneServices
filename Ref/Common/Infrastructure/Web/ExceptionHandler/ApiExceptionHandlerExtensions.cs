using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.Common.Web.ExceptionHandler
{
	public static class ApiExceptionHandlerExtensions
	{
		public static IApplicationBuilder UseCommonApiExceptionHandler(this IApplicationBuilder app, bool showDetailsForLoggedInUser = true, Action<HttpContext, ApiLogException> handler = null)
		{
			return app.UseExceptionHandler(errorApp =>
			{
				errorApp.Run(async context =>
				{
					await HandleException(context, handler, showDetailsForLoggedInUser);
				});
			});
		}

		static async Task HandleException(HttpContext context, Action<HttpContext, ApiLogException> handler, bool showDetailsForLoggedInUser)
		{
			var userId = context.GetUserId();
			var exception = context.Features.Get<IExceptionHandlerFeature>().Error;
			var requestUri = context.Request.GetEncodedPathAndQuery();
			var apiLogException = new ApiLogException
			{
				UserId = userId,
				RequestUri = requestUri,
				Exception = exception
			};

			handler?.Invoke(context, apiLogException);

			var log = context.RequestServices.GetRequiredService<ILogWrapper>()?.GetLog<ExceptionHandlerMiddleware>() ?? throw new InvalidOperationException("Fail to get Log.");
			var userIdLog = string.IsNullOrEmpty(userId) ? "" : $" - User {userId}";
			log.Error($"Exception in {requestUri}{userIdLog}", exception);

			var errorMessage = !string.IsNullOrEmpty(userId) && showDetailsForLoggedInUser ? exception.GetUnWrappedMessage() : GenericErrorMessage;
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			context.Response.ContentType = MediaTypeNames.Text.Plain;
			await context.Response.WriteAsync(errorMessage);
		}

		const string GenericErrorMessage = "An unexpected server error occurred.";
	}
}
