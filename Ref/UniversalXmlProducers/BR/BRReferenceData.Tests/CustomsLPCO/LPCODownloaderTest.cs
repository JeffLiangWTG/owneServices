using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class LPCODownloaderTest
	{
		[Test]
		public void TestDownloadWithMock()
		{
			TokenDTO tokenDTO = new TokenDTO() { XToken = "token", Authorization = "authorization" };
			using (var httpMock = new MockHttpMessageHandler())
			{
				var jsonResponse = Utils.GetManifestResourceStream(
					"CargoWise.RefDbRepo.BRReferenceData.Tests.CustomsLPCO.TestFiles.Input.Modelo_E00126.json");
				httpMock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LPCO_MODEL"] + model).Respond("application/json", jsonResponse);

				using (var client = httpMock.ToHttpClient())
				{
					var result = LPCODownloader.DownloadModel(model, client, tokenDTO);

					using (var expectedResult = Utils.GetManifestResourceStream(
						"CargoWise.RefDbRepo.BRReferenceData.Tests.CustomsLPCO.TestFiles.Input.Modelo_E00126.json"))
					{
						Assert.IsNotNull(expectedResult);
						using (var expectedText = new StreamReader(expectedResult))
						{
							Assert.AreEqual(expectedText.ReadToEnd(), result);
						}
					}
				}
			}
		}

		readonly string model = "E00126";
	}
}
