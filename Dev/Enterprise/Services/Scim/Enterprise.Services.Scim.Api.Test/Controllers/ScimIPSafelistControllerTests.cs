using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Business;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test.Controllers
{
	[TestFixture]
	sealed class ScimIPSafelistControllerTests : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		public void TestIPSafelist_SafeListNone_Succeeds()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.None))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString());

				var response = GetAsyncRequest(requestBuilder);
				AssertNotEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestIPSafelist_SucceedsWithValidIP()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");
			MemoryCache.Default.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, new[] { "20.50.76.176/32", "2.2.2.2/32" }, new CacheItemPolicy());

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "20.50.76.176");

				var response = GetAsyncRequest(requestBuilder);
				AssertNotEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestIPSafelist_MultipleIPs_Last()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");
			MemoryCache.Default.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, new[] { "20.50.76.176/32", "2.2.2.2/32" }, new CacheItemPolicy());

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "1.1.1.1, 20.50.76.176");

				var response = GetAsyncRequest(requestBuilder);
				AssertNotEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestIPSafelist_MultipleIPs_Last_DifferentHeaders()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");
			MemoryCache.Default.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, new[] { "20.50.76.176/32", "2.2.2.2/32" }, new CacheItemPolicy());

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "1.1.1.1")
					.AddHeader("X-Forwarded-For", "20.50.76.176");

				var response = GetAsyncRequest(requestBuilder);
				AssertNotEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestIPSafelist_MultipleIPs_First()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");
			MemoryCache.Default.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, new[] { "20.50.76.176/32", "2.2.2.2/32" }, new CacheItemPolicy());

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "20.50.76.176, 1.1.1.1");

				var response = GetAsyncRequest(requestBuilder);
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestIPSafelist_FailsWithInvalidIP()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");
			MemoryCache.Default.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, new[] { "1.1.1.1/32", "2.2.2.2/32" }, new CacheItemPolicy());

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "1.1.1.3");

				var response = GetAsyncRequest(requestBuilder);
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
			}
		}

		[UseSnapshotProtection]
		public void TestIPSafelist_FailsWithNoIP()
		{
			var testToken = Guid.NewGuid();
			TestingState.IsRunningTests = true;
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");
			MemoryCache.Default.Set(IPSafelistHelper.SAFELIST_CACHE_KEY, new[] { "1.1.1.1/32", "2.2.2.2/32" }, new CacheItemPolicy());

			using (SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.ScimAuthenticationMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.AuthenticationCodes.ApiToken))
			using (SystemDataRegistry.Instance.ScimApiTokenAuthentication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testToken.ToString()))
			using (SystemDataRegistry.Instance.ScimSafeListType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ScimConstants.SafelistTypes.AzureEntra))
			using (var server = TestServer.Create(app =>
			{
				var startup = new TestStartup();
				startup.Configuration(app);
			}))
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest($"/scim/Users/{Guid.NewGuid()}")
					.AddHeader("Authorization", "Bearer " + testToken.ToString())
					.AddHeader("X-Forwarded-For", "");

				var response = GetAsyncRequest(requestBuilder);
				AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
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
