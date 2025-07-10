using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class LPCOConsentingBodyDownloaderTest
	{
		[Test]
		public void TestDownloadWithMock()
		{
			TokenDTO tokenDTO = new TokenDTO() { XToken = "token", Authorization = "authorization" };
			using (var httpMock = new MockHttpMessageHandler())
			{
				var jsonResponse = Utils.GetManifestResourceStream(
					"CargoWise.RefDbRepo.BRReferenceData.Tests.CustomsLPCO.TestFiles.Input.ANP.json");
				httpMock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LPCO_MODEL_SEARCH"] + model).Respond("application/json", jsonResponse);

				using (var client = httpMock.ToHttpClient())
				{
					var dtos = LPCOConsentingBodyDownloader.DownloadModelsFromConsentingBody(client, tokenDTO);
					Assert.AreEqual(3, dtos.Count);
					Assert.AreEqual("E00016", dtos[0].codigo);
					Assert.AreEqual("I00020", dtos[1].codigo);
					Assert.AreEqual("I00024", dtos[2].codigo);
				}
			}
		}

		readonly string model = "ANP";
	}
}
