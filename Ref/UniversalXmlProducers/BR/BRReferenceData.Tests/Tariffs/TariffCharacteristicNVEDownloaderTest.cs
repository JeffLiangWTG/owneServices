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
	class TariffCharacteristicNVEDownloaderTest
	{
		[Test]
		public void DownloadXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NVE_ATTRIBUTE"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.NVE_GET.html"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TARIFF_NVE_ATTRIBUTE"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve.xml"));

				CaptchaSolvingUtils.
				Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var bXml = TariffCharacteristicNVEDownloader.Download(client);

					Assert.NotNull(bXml, nameof(bXml));
					using (var expectedText = new StreamReader(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve.xml"), Encoding.UTF8))
					using (var outputText = new StreamReader(new MemoryStream(bXml), Encoding.UTF8))
					{
						Assert.AreEqual(expectedText.ReadToEnd(), outputText.ReadToEnd());
					}
				}
			}
		}
	}
}
