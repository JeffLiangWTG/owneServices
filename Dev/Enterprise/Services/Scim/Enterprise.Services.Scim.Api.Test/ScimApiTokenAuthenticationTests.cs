using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Tests.Helpers;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	sealed class ScimApiTokenAuthenticationTests : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestScimAuthenticationMethod_Token_WithCorrectToken_Succeeds()
		{
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");

			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (var server = TestServer.Create<TestStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
				.AddHeader("Authorization", "Bearer " + testToken.ToString())
				.AddHeader("X-Forwarded-For", "127.0.0.1");

				var response = GetAsyncRequest(requestBuilder);

				AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestScimException_ReturnErrorWithoutStackTrace()
		{
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");

			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (var server = TestServer.Create<TestStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest("/scim/Users/1")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "127.0.0.1");

				var response = GetAsyncRequest(requestBuilder);
				var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				var jToken = Utils.ExtractJsonFromBody(responseContent);

				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertEquals("Bad id: 1", jToken.GetTokenAsString("detail"));
			}
		}

		[UseSnapshotProtection]
		public void TestScimAuthenticationMethod_Token_WithIncorrectToken_ReturnsUnauthorized()
		{
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");

			TestingState.IsRunningTests = true;

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid().ToString()))
			using (var server = TestServer.Create<TestStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest("/scim/Users/1")
					.AddHeader("Authorization", "Bearer wrong_token")
					.AddHeader("X-Forwarded-For", "127.0.0.1");

				var response = GetAsyncRequest(requestBuilder);

				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			}
		}

		HttpResponseMessage GetAsyncRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.GetAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}

		protected override void SetUp()
		{
			ConfigurationManager.AppSettings["Issuer"] = "";
			ConfigurationManager.AppSettings["AudienceId"] = "";
			ConfigurationManager.AppSettings["AudienceSecret"] = "";
			ConfigurationManager.AppSettings["ServerName"] = System.Environment.MachineName;
			ConfigurationManager.AppSettings["DatabaseName"] = "Odyssey";
			ConfigurationManager.AppSettings["SchemaPath"] = "Schemas";
			base.SetUp();
		}

		protected override void TearDown()
		{
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			finally
			{
				base.TearDown();
			}
		}
	}
}
