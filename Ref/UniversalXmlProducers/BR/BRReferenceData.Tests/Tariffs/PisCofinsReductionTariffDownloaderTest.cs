using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class PisCofinsReductionTariffDownloaderTest
	{
		[Test]
		public void DownloadBaseCalculationReductionPisCofinsXMLWithMock()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_BASE_CALCULATION_REDUCTION_PIS_COFINS"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.PIS_COFINS.BASE_CALCULATION_REDUCTION_PIS_CONFINS_GET.txt"));
				mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_BASE_CALCULATION_REDUCTION_PIS_COFINS"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.PIS_COFINS.FundamentoLegalReducaoPisCofins.xml"));

				CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

				using (var client = mockHttp.ToHttpClient())
				{
					var bXMl = PisCofinsReductionTariffDownloader.Download(client);

					Assert.NotNull(bXMl, nameof(bXMl));
					using (var expectedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.PIS_COFINS.FundamentoLegalReducaoPisCofins.xml"))
					using (var outputStream = new MemoryStream(bXMl))
					{
						Assert.AreEqual(expectedStream, outputStream);
					}
				}
			}
		}
	}
}
