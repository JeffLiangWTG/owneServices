using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class SpecialClearanceAttributesProcedureDownloaderTest
	{
		[Test]
		public void TestDownloadWithMock()
		{
			using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.enq_due.xlsx"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.enq_due.xlsx"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_SPECIAL_CLEARANCE_ATTRIBUTES"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Procedures.TestFiles.Input.TRATAMENTO_ADMINISTRATIVO_GET.html"));
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/siscomex/pt-br/informacoes/tratamento-administrativo-1/tratamento-administrativo-de-exportacao-1/enq_due.xlsx").Respond("application/xlsx", stream);

					using (var client = mockHttp.ToHttpClient())
					{
						var downloader = new SpecialClearanceAttributesProcedureDownloader();

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
