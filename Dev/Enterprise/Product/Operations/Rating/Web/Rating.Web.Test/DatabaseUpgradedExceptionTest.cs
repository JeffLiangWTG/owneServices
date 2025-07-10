using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Rating.Web.Configuration;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test
{
	public class DatabaseUpgradedExceptionTest : TestCase
	{
		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDatabaseUpgradeOnStartup()
		{
			using (Db.DisposableUpgrade_ForTest())
			{
				using (var ts = TestServer.Create<RatingAPIsStartup>())
				{ }

				Assert("No uncaught DatabaseUpgradedException", condition: true);
			}
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDatabaseUpgradeOnAuthentication()
		{
			using (Db.DisposableUpgrade_ForTest())
			{
				using (var server = TestServer.Create<RatingAPIsStartup>())
				{
					AssertNotNull(server);

					var requestBuilder = server.CreateRequest("");
					// any valid JWT token: we just need to trigger the
					// authentication flow, which accesses Registry and is
					// therefore sensitive to database upgrades
					const string dummyJWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
					requestBuilder.And(configure => configure.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dummyJWT));

					var response = PostAsyncRequest(requestBuilder);
					var content = response.Content.ReadAsStringAsync().Result;

					AssertEquals(HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertEquals("Upgrade in progress", content);
				}
			}
		}

		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		public void TestDatabaseUpgradeOnWebApi()
		{
			using (Db.DisposableUpgrade_ForTest())
			{
				using (var server = TestServer.Create<RatingAPIsStartup>())
				{
					AssertNotNull(server);

					var requestBuilder = server.CreateRequest("wtg/status");
					var response = GetAsyncRequest(requestBuilder);
					var content = response.Content.ReadAsStringAsync().Result;

					AssertEquals(HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertEquals("Upgrade in progress", content);
				}
			}
		}

		HttpResponseMessage PostAsyncRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.PostAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}

		HttpResponseMessage GetAsyncRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.GetAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}
	}
}
