using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	public class TariffRateLetecDownloaderTest
	{
		[Test]
		public void TestDownloadWithMock()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_CURRENT_LISTS_TARIFF_RATES"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.LISTAS_VIGENTES_GET.html"));
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_v_letec.xlsx").Respond("application/xlsx", stream);

					using (var client = mockHttp.ToHttpClient())
					{
						var downloader = new TariffRateLetecDownloader();

						var bFile = downloader.Download(client);
						using (var downloadedStream = new MemoryStream(bFile))
						{
							Assert.AreEqual(downloadedStream, expectedStream);
						}
					}
				}
			}
		}
	}
}
