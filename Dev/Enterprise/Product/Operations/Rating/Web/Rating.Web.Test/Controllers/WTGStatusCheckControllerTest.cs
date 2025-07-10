using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Rating.Web.Configuration;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Controllers
{
	public class WTGStatusCheckControllerTest : TestCase
	{
		[DeveloperOnlyTest]
		[UseSnapshotProtection]
		public void TestWTGStatusEndPoint_WhenDBIsNotAvailable_ReturnUnavailable()
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

		public void TestWTGStatusEndPoint_WhenDBIsAvailable_ReturnOK()
		{
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				AssertNotNull(server);
				var requestBuilder = server.CreateRequest("wtg/status");

				var response = GetAsyncRequest(requestBuilder);
				Assert(response.IsSuccessStatusCode);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
			}
		}

		HttpResponseMessage GetAsyncRequest(RequestBuilder requestBuilder)
		{
			return AsyncHelper.RunTask(() => requestBuilder.GetAsync(), CancellationToken.None, "Hit Endpoint").GetAwaiter().GetResult().Result;
		}
	}
}
