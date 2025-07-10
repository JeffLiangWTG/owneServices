#if NETFRAMEWORK
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;
#elif NET
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public static class HttpActionResultAssertions
	{
#if NETFRAMEWORK
		public static void AssertJsonResultEquals(this IHttpActionResult actionResult, object expected, JsonSerializerSettings jsonSerializerSettings = null)
		{
			var expectedJson = JsonConvert.SerializeObject(expected, jsonSerializerSettings);

			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var actualJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assertion.AssertEquals(expectedJson, actualJson);
		}

		public static void AssertJsonResultContains(this IHttpActionResult actionResult, object expected, JsonSerializerSettings jsonSerializerSettings = null)
		{
			var expectedJson = JsonConvert.SerializeObject(expected, jsonSerializerSettings);

			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var actualJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assertion.AssertContains(expectedJson, actualJson);
		}

		public static void AssertResultEquals(this IHttpActionResult actionResult, HttpStatusCode expectedStatusCode, string expectedMessage = null)
		{
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			if (expectedMessage != null)
			{
				var actualMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				Assertion.AssertEquals(expectedMessage, actualMessage);
			}

			Assertion.AssertEquals(expectedStatusCode, response.StatusCode);
		}

		public static void AssertResultContains(this IHttpActionResult actionResult, HttpStatusCode expectedStatusCode, string expectedMessage = null)
		{
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			if (expectedMessage != null)
			{
				var actualMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				Assertion.AssertContains(expectedMessage, Regex.Unescape(actualMessage));
			}

			Assertion.AssertEquals(expectedStatusCode, response.StatusCode);
		}

		public static void AssertResultIsString(this IHttpActionResult actionResult)
		{
			if (actionResult is ResponseMessageResult responseMessage)
			{
				Assertion.AssertType(typeof(StringContent), responseMessage.Response.Content);
			}
			else if (actionResult is not NegotiatedContentResult<string> negotiatedContentResult)
			{
				Assert.Fail("The provided action result was not a string");
			}
			else
			{
				Assert.Pass();
			}
		}

#elif NET
		public static void AssertJsonResultEquals(this IActionResult actionResult, object expected, JsonSerializerSettings jsonSerializerSettings = null)
		{
			var expectedJson = JsonConvert.SerializeObject(expected, jsonSerializerSettings);
			var actualJson = GetJson(actionResult, jsonSerializerSettings);

			Assertion.AssertEquals(expectedJson, actualJson);
		}

		public static void AssertJsonResultContains(this IActionResult actionResult, object expected, JsonSerializerSettings jsonSerializerSettings = null)
		{
			var expectedJson = JsonConvert.SerializeObject(expected, jsonSerializerSettings);
			var actualJson = GetJson(actionResult, jsonSerializerSettings);

			Assertion.AssertContains(expectedJson, actualJson);
		}

		static string GetJson(IActionResult actionResult, JsonSerializerSettings jsonSerializerSettings = null)
		{
			return actionResult switch
			{
				ObjectResult result => JsonConvert.SerializeObject(result?.Value, jsonSerializerSettings),
				ContentResult result => result.Content,
				_ => throw new ArgumentException("The action result is not a type that has content that can be extract as JSON.")
			};
		}

		public static void AssertResultEquals(this IActionResult actionResult, HttpStatusCode expectedStatusCode, string expectedMessage = null)
		{
			Assert.Multiple(() =>
			{
				if (expectedMessage != null)
				{
					AssertContent(actionResult, expectedMessage);
				}
				AssertStatusCode(actionResult, expectedStatusCode);
			});
		}

		static void AssertContent(IActionResult actionResult, string expectedMessage)
		{
			var actualMessage = GetContentMessage(actionResult);
			Assertion.AssertEquals(expectedMessage, actualMessage);
		}

		public static void AssertResultContains(this IActionResult actionResult, HttpStatusCode expectedStatusCode, string expectedMessage = null)
		{
			Assert.Multiple(() =>
			{
				if (expectedMessage != null)
				{
					AssertContentContains(actionResult, expectedMessage);
				}
				AssertStatusCode(actionResult, expectedStatusCode);
			});
		}

		static void AssertContentContains(IActionResult actionResult, string expectedMessage)
		{
			var actualMessage = GetContentMessage(actionResult);
			//https://github.com/dotnet/aspnetcore/issues/37283 - Inconsistant bool to string conversion in .Net Framework vs .Net Core
			Assertion.AssertContains(expectedMessage, Regex.Unescape(actualMessage), ignoreCase: true);
		}

		static string GetContentMessage(IActionResult actionResult)
		{
			var responseObject = actionResult switch
			{
				ObjectResult result => result?.Value,
				ContentResult result => result.Content,
				_ => throw new ArgumentException("The action result is not a type can have its content extracted.")
			};

			return responseObject switch
			{
				string responseString => responseString,
				_ => JsonConvert.SerializeObject(responseObject)
			};
		}

		static void AssertStatusCode(IActionResult actionResult, HttpStatusCode expectedStatusCode)
		{
			var statusCodeResult = actionResult as IStatusCodeActionResult;
			Assertion.AssertEquals((int)expectedStatusCode, statusCodeResult?.StatusCode);
		}

		public static void AssertResultIsString(this IActionResult actionResult)
		{
			Assertion.AssertType(typeof(ContentResult), actionResult);
		}
#endif
	}
}
