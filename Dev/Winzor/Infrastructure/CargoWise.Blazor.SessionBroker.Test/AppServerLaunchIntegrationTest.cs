using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.Common.Testing.Auth;
using CargoWise.Blazor.Testing.Common;
using CargoWiseNext.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	[KillBlazorAppProcesses]
	public class AppServerLaunchIntegrationTest
	{
		[Test]
		public async Task TestLaunch()
		{
			using var factory = new CustomWebApplicationFactory<Startup>();
			var options = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			var token = new DebugClientTokenGenerator(options).GenerateClientToken();
			using var client = factory.CreateClient(new () { AllowAutoRedirect = false });
			var response1 = await client.GetAsync($"/error?{QueryParameters.ClientToken}={token}");
			Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
			Assert.That(response1.Headers.Location.OriginalString, Does.StartWith("/error"));
			const string cookieHeaderPrefix = ".Yarp.ReverseProxy.Affinity=";
			Assert.That(response1.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies));
			var affinityCookieHeader = cookies.Single(c => c.StartsWith(cookieHeaderPrefix));
			var affinityCookieValue = affinityCookieHeader.Substring(cookieHeaderPrefix.Length);
			Assert.That(affinityCookieValue, Is.Not.Null.Or.Empty, "Affinity cookie should have been set");
			var response2 = await client.GetAsync(response1.Headers.Location);
			Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		}
	}
}
