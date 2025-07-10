using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	public class TariffDetachesTest
	{
		[Test]
		public void TestTariffDetachesDownload()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var oHttpClient = mockHttp.ToHttpClient();

				using (var outputFile = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TariffDetaches.TA-IMP-AnuenteWeb-atual-09042022.xlsx"))
				using (var testFile = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.TariffDetaches.TRATAMENTO_ADMINISTRATIVO_GET.html"))
				{
					var url = ConfigurationProvider.Configuration["URL_TARIFF_DETACHES"];

					mockHttp.When(HttpMethod.Get, url).Respond("application/html", testFile);
					mockHttp.When(HttpMethod.Get, "http://www.siscomex.gov.br/wp-content/uploads/2022/04/TA-IMP-AnuenteWeb-atual-09042022.xlsx").Respond("application/xlsx", outputFile);

					var downloader = new TariffDetachesDownloader();
					byte[] bXml = downloader.Download(oHttpClient);
					using (var downloadedFile = new MemoryStream(bXml))
					{
						Assert.AreEqual(outputFile, downloadedFile);
					}
				}
			}
		}
	}
}
