using Newtonsoft.Json.Linq;
#if NETFRAMEWORK
using System.Threading;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace Enterprise.Services.ServiceHost.Tests
{
	public static class HttpActionResultExtensions
	{
		public static JToken GetJsonResult(this IActionResult actionResult)
		{
			return JToken.Parse(actionResult.GetStringResult());
		}

		public static string GetStringResult(this IActionResult actionResult)
		{
#if NET
			var response = actionResult as OkObjectResult;
			return response.Value as string;
#else
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
#endif
		}
	}
}
