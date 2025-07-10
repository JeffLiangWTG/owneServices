#if NETFRAMEWORK
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using IActionResult = System.Web.Http.IHttpActionResult;
#elif NET
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
#endif
using System;
using System.Net;
using System.Net.Http;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost.Tests
{
	[CodeAlive("ServiceHost upgrade to .NET 8. Delete the class once fully upgraded.")]
	public static class ResultExtensions
	{
#if NETFRAMEWORK
		public static HttpResponseMessage GetResult(this IActionResult result)
		{
			return result.ExecuteAsync(CancellationToken.None).Result;
		}

		public static HttpResponseMessage GetResult(this Task<HttpResponseMessage> result)
		{
			return result.GetAwaiter().GetResult();
		}

		public static int GetStatusCode(this HttpResponseMessage result)
		{
			return (int)result.StatusCode;
		}

		public static string GetMessage(this HttpResponseMessage result, HttpStatusCode statusCode = default, bool isJson = false)
		{
			return result.Content?.ReadAsStringAsync().Result;
		}

		public static object GetContent(this HttpResponseMessage result, HttpStatusCode statusCode = default)
		{
			return result.Content;
		}

		public static object GetValue(this HttpResponseMessage result, HttpStatusCode statusCode = default)
		{
			return ((ObjectContent)result.Content).Value;
		}

		public static T JsonResult<T>(this IActionResult actionResult, HttpStatusCode statusCode = default)
		{
			var result = actionResult as OkNegotiatedContentResult<T>;

			if (result is OkNegotiatedContentResult<T> jsonResult)
			{
				return jsonResult.Content;
			}

			return default;
		}

		public static Exception GetException(this IActionResult result)
		{
			return ((ExceptionResult)result)?.Exception;
		}
#elif NET
		public static IActionResult GetResult(this IActionResult result)
		{
			return result;
		}

		public static int GetStatusCode(this IActionResult result)
		{
			var statusCodeProperty = result.GetType().GetProperty("StatusCode");
			if (statusCodeProperty != null)
			{
				var value = statusCodeProperty.GetValue(result);
				if (value is int statusCode)
				{
					return statusCode;
				}
			}

			return 0;
		}

		public static HttpStatusCode GetStatus(this HttpResponseMessage result, HttpStatusCode statusCode = default)
		{
			return result.StatusCode;
		}

		public static string GetMessage(this IActionResult result, HttpStatusCode statusCode = default, bool isJson = false)
		{
			return statusCode switch
			{
				HttpStatusCode.BadRequest => isJson
					? JsonConvert.SerializeObject(((BadRequestObjectResult)result).Value)
					: ((BadRequestObjectResult)result).Value?.ToString(),
				HttpStatusCode.OK => result switch
				{
					OkObjectResult okObjectResult => isJson
						? JsonConvert.SerializeObject(okObjectResult.Value)
						: okObjectResult.Value?.ToString(),
					ContentResult contentResult => contentResult.Content,
					_ => ""
				},
				_ => ""
			};
		}

		public static object GetContent(this IActionResult result, HttpStatusCode statusCode = default)
		{
			switch (statusCode)
			{
				case HttpStatusCode.BadRequest:
					return ((BadRequestObjectResult)result).Value;
				case HttpStatusCode.OK:
					if (result is OkObjectResult okObjectResult)
					{
						return okObjectResult.Value;
					}
					break;
			}

			return null;
		}

		public static object GetValue(this IActionResult result, HttpStatusCode statusCode = default)
		{
			return GetContent(result, statusCode);
		}

		public static T JsonResult<T>(this IActionResult result, HttpStatusCode statusCode = default)
		{
			return (T)GetContent(result, statusCode);
		}

		public static Exception GetException(this IActionResult result)
		{
			return ((ObjectResult)result).Value as Exception;
		}
#endif
	}
}
