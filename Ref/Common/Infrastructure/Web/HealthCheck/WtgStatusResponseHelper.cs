using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Primitives;

namespace CargoWise.RefDbRepo.Common.Web
{
	public static class WtgStatusResponseHelper
	{
		public static async Task WriteWtgStatusResponse(HttpContext context, HealthReport healthReport)
		{
			var requestMethod = context.Request.Method;
			if (HttpMethods.IsGet(requestMethod) || HttpMethods.IsHead(requestMethod))
			{
				SetContentType(context);
				context.Response.StatusCode = StatusCodes.Status200OK;
				var contents = healthReport.Entries.Select(x => $"{MapStatus(x.Value.Status)}({x.Key}): {ReplaceEOL(x.Value.Description)}");
				await context.Response.WriteAsync(string.Join("\r\n", contents));
			}
			else
			{
				SetAllowedMethods(context);
				context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
			}
			await context.Response.CompleteAsync();
		}

		static void SetContentType(HttpContext context)
		{
			context.Response.ContentType = "text/plain; charset=utf-8";
		}

		static void SetAllowedMethods(HttpContext context)
		{
			context.Response.Headers.Allow = new StringValues(new[] { HttpMethods.Get, HttpMethods.Head });
		}

		static string ReplaceEOL(string message)
		{
			return message.Replace("\r", " ").Replace("\n", " ");
		}

		static string MapStatus(HealthStatus healthStatus)
		{
			switch (healthStatus)
			{
				case HealthStatus.Healthy:
					return INFO;
				case HealthStatus.Degraded:
					return WARNING;
				case HealthStatus.Unhealthy:
					return ERROR;
				default:
					throw new NotSupportedException();
			}
		}

		const string INFO = "INFO";
		const string WARNING = "WARNING";
		const string ERROR = "ERROR";
	}
}
