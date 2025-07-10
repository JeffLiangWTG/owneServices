using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class PortalUnicoAuthenticatorTest
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestAuthentication(bool isProduction)
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes("{token}")))
				{
					using (var responseMessageLogin = new HttpResponseMessage(HttpStatusCode.OK))
					{
						responseMessageLogin.Headers.Add("X-CSRF-Token", "Test-Token");
						responseMessageLogin.Headers.Add("Set-Token", "Test-Authorization");
						mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration[isProduction ? "URL_PORTAL_UNICO_AUTHENTICATION" : "URL_PORTAL_UNICO_AUTHENTICATION_TEST"]).Respond(req => responseMessageLogin);

						using (var client = mockHttp.ToHttpClient())
						{
							var tokenDTO = PortalUnicoAuthenticator.DoAuthentication(client, isProduction);
							Assert.AreEqual("Test-Token", tokenDTO.XToken);
							Assert.AreEqual("Test-Authorization", tokenDTO.Authorization);
						}
					}
				}
			}
		}
	}
}
