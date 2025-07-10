using System.Linq;
using NUnit.Framework;
#if NET
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
#elif NETFRAMEWORK
using System.Threading;
using System.Web.Http.Results;
using IActionResult = System.Web.Http.IHttpActionResult;
#endif

namespace Enterprise.Services.ServiceHost.Tests
{
	public static class AssertHelper
	{
#if NET
		public static void AssertResult(string expected, IActionResult result)
		{
			Assertion.AssertNotNull(result);
			var okResult = result as OkObjectResult;
			Assertion.AssertEquals(expected, okResult.Value);
		}

		public static void AssertModelState(string key, string errMsg, IActionResult actionResult)
		{
			var result = actionResult as BadRequestObjectResult;
			var modelStateValue = (result.Value as SerializableError)?.GetValueOrDefault(key) as string[];
			Assertion.AssertEquals(modelStateValue?.Single(), errMsg);
		}

#elif NETFRAMEWORK
		public static void AssertResult(string expected, IActionResult result)
		{
			var controllerResult = result.ExecuteAsync(CancellationToken.None).Result;
			Assertion.AssertNotNull(controllerResult);
			Assertion.AssertNotNull(result);
			Assertion.AssertEquals(expected, controllerResult.Content.ReadAsStringAsync().Result.Trim('"'));
		}

		public static void AssertModelState(string key, string errMsg, IActionResult actionResult)
		{
			var result = actionResult as InvalidModelStateResult;
			var error = result.ModelState[key].Errors.Single();
			Assertion.AssertEquals(error.ErrorMessage, errMsg);
		}
#endif
	}
}
