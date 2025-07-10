using System.Net;
using System.Threading.Tasks;
using Enterprise.Rating.Web.Configuration;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Controllers
{
	public class RootControllerTest : TransactionedTestCase
	{
		public void TestRootRedirectsToSwaggerDocs()
		{
			using (var server = TestServer.Create<RatingAPIsStartup>())
			{
				var rb = server.CreateRequest("/");
				var response = Task.Run(rb.GetAsync).Result;

				AssertEquals(HttpStatusCode.Found, response.StatusCode);
				AssertEquals("http://localhost/swagger/ui/index", response.Headers.Location);
			}
		}
	}
}
