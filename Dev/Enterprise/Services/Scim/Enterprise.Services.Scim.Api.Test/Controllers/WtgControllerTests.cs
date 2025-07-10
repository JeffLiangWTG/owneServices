using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test.Controllers
{
	[TestFixture]
	public class WtgControllerTests
	{
		[Test]
		public async Task TestWtgStatusWithGetShouldReturn200()
		{
			using (var server = TestServer.Create<TestStartup>())
			{
				Assert.That(server, Is.Not.Null);

				var response = await server.HttpClient.GetAsync("/wtg/status");
				var result = await response.Content.ReadAsStringAsync();

				Assert.That(response.IsSuccessStatusCode, Is.True);
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			}
		}

		[Test]
		public async Task TestWtgStatusWithHeadShouldReturn200()
		{
			using (var server = TestServer.Create<TestStartup>())
			{
				Assert.That(server, Is.Not.Null);

				var response = await server.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/wtg/status"));
				var result = await response.Content.ReadAsStringAsync();

				Assert.That(response.IsSuccessStatusCode, Is.True);
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			}
		}

		[SetUp]
		public void Init()
		{
			ConfigurationManager.AppSettings["Issuer"] = "";
			ConfigurationManager.AppSettings["AudienceId"] = "";
			ConfigurationManager.AppSettings["AudienceSecret"] = "";
			ConfigurationManager.AppSettings["ServerName"] = System.Environment.MachineName;
			ConfigurationManager.AppSettings["DatabaseName"] = "Odyssey";
			ConfigurationManager.AppSettings["SchemaPath"] = "Schemas";
		}
	}
}
