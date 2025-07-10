using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	[TestFixture]
	internal class ResponseHeaderTests
	{
		[Test]
		public async Task TestShouldHideSoftwareVersion()
		{
			using (var server = TestServer.Create<TestStartup>())
			{
				Assert.That(server, Is.Not.Null);

				var response = await server.HttpClient.GetAsync("/scim/Users/1");
				var result = await response.Content.ReadAsStringAsync();
				var headers = response.Content.Headers;

				Assert.That(headers.Contains("X-AspNet-Version"), Is.False);
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
