using System.Linq;
using System.Net;
using System.Net.Http;
using WTG.TestHelpers.IISExpress;

namespace Enterprise.Services.ServiceHost.Tests
{
	[RequiresIISExpress]
	class SecurityHeadersTest : IntegrationTestBase
	{
		public void TestHeaders()
		{
			TestSecurityHeaders("/");
			TestSecurityHeaders("/favicon.ico", expectedStatusCode: null);
			TestSecurityHeaders("/nonexistingurl", expectedStatusCode: HttpStatusCode.NotFound);
			TestSecurityHeaders("/", HttpMethod.Post, expectedStatusCode: null);
		}

		public void TestCSPHeadersExist()
		{
			var request = CreateRequest(HttpMethod.Get, "/");

			var response = HttpClient.SendAsync(request).ConfigureAwait(true).GetAwaiter().GetResult();
			AssertEquals($"Required CSP Headers do not exist", true, response.Headers.Any(h => h.Key == "Content-Security-Policy"
				&& h.Value.Contains("default-src 'none'; upgrade-insecure-requests; style-src 'self'; script-src 'self'; img-src 'self'; frame-ancestors 'none'")));
		}

		public void TestCacheHeaderExistsForDynamicResource()
		{
			var request = CreateRequest(HttpMethod.Get, "/");
			var response = HttpClient.SendAsync(request).ConfigureAwait(true).GetAwaiter().GetResult();
			AssertEquals($"Required Cache-Control header does not exist", true, response.Headers.Any(h => h.Key == "Cache-Control" && h.Value.Contains("no-store, no-cache")));
		}

		public void TestCacheHeaderExistsForStaticResource()
		{
			var request = CreateRequest(HttpMethod.Get, "/TrampolineLandingPageStyle");
			var response = HttpClient.SendAsync(request).ConfigureAwait(true).GetAwaiter().GetResult();
			AssertEquals($"Required Cache-Control header does not exist", true, response.Headers.Any(h => h.Key == "Cache-Control" && h.Value.Contains("no-cache=\"Set-Cookie\"")));
		}
	}
}
