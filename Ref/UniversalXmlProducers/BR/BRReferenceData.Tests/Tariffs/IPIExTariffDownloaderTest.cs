using System.IO;
using System.Net.Http;
using System.Text;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class IPIExTariffDownloaderTest
	{
		[Test]
		public void TestDownloadXlsxWithMock()
		{
			using (var html = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ExTariff_IPI.ExTariffIPI.html"))
			using (var xlsx = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ExTariff_IPI.TIPI 2022 - Atualizada ADE 005-2022.xlsx"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TIPI_DOCS"]).Respond("application/html", html);
					mockHttp.When(HttpMethod.Post, $"https://www.gov.br/receitafederal/pt-br/acesso-a-informacao/legislacao/documentos-e-arquivos/tipi.xlsx/@@download/file").Respond("application/file", xlsx);

					using (var client = mockHttp.ToHttpClient())
					{
						var bXml = IPIExTariffDownloader.Download(client);

						Assert.NotNull(bXml, nameof(bXml));
						using (var expectedText = new StreamReader(Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ExTariff_IPI.TIPI 2022 - Atualizada ADE 005-2022.xlsx"), Encoding.UTF8))
						using (var outputText = new StreamReader(new MemoryStream(bXml), Encoding.UTF8))
						{
							Assert.AreEqual(expectedText.ReadToEnd(), outputText.ReadToEnd());
						}
					}
				}
			}
		}

		[Test]
		public void TestGetUpdates()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TIPI_DOCS"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ExTariff_IPI.ExTariffIPI.html"));

				using (var client = mockHttp.ToHttpClient())
				{
					var updatesString = IPIExTariffDownloader.DownloadAttributesHtml(client);

					Assert.NotNull(updatesString);
				}
			}
		}
	}
}
