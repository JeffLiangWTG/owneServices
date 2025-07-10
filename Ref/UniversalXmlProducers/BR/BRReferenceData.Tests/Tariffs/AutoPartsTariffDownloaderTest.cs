using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class AutoPartsTariffDownloaderTest
	{
		[Test]
		public void TestDownloadWithMock()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022.xlsx"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022.xlsx"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_AUTO_PARTS_LIST"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.AUTO_PART_LIST_GET.html"));
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/competitividade-industrial/setor-automotivo/regime-autopecas/documentos-regime-de-autopecas/ListaGeraldeAutopeasNoProduzidasv24032022.xlsx")
						.Respond("application/xlsx", stream);

					using (var client = mockHttp.ToHttpClient())
					{
						var downloader = new AutoPartsTariffDownloader();

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
