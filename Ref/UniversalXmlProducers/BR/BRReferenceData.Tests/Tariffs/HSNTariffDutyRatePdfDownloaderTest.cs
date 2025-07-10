using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class HSNTariffDutyRatePdfDownloaderTest
	{
		[Test]
		public void TestCompareDownloadedFiles()
		{
			using (var outputFile_AnnexII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.Anexo_II_Res_272_2021.xlsx"))
			using (var outputFile_AnnexIV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_iv_desabastecimento.xlsx"))
			using (var outputFile_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			using (var outputFile_AnnexVI = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vi_lebit_bk.xlsx"))
			using (var outputFile_AnnexVII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vii_lista_covid.xlsx"))
			{
				using (var mockHttp = new MockHttpMessageHandler())
				{
					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_CURRENT_LISTS_TARIFF_RATES"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.LISTAS_VIGENTES_GET.html"));
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/Anexo_II_Res_272_2021.xlsx").Respond("application/xlsx", outputFile_AnnexII);
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_iv_desabastecimento.xlsx").Respond("application/xlsx", outputFile_AnnexIV);
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_v_letec.xlsx").Respond("application/xlsx", outputFile_AnnexV);
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_vi_lebit_bk.xlsx").Respond("application/xlsx", outputFile_AnnexVI);
					mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_vii_lista_covid.xlsx").Respond("application/xlsx", outputFile_AnnexVII);
					using (var client = mockHttp.ToHttpClient())
					{
						var downloader = new HSNTariffDutyRatePdfDownloader();
						var bXslx = downloader.Download(client);
						Assert.AreEqual(5, bXslx.Count);

						using (var expectedStream_AnnexII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.Anexo_II_Res_272_2021.xlsx"))
						using (var expectedStream_AnnexIV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_iv_desabastecimento.xlsx"))
						using (var expectedStream_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
						using (var expectedStream_AnnexVI = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vi_lebit_bk.xlsx"))
						using (var expectedStream_AnnexVII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vii_lista_covid.xlsx"))
						using (var outputStream_AnnexII = new MemoryStream(bXslx[4]))
						using (var outputStream_AnnexIV = new MemoryStream(bXslx[3]))
						using (var outputStream_AnnexV = new MemoryStream(bXslx[2]))
						using (var outputStream_AnnexVI = new MemoryStream(bXslx[1]))
						using (var outputStream_AnnexVII = new MemoryStream(bXslx[0]))
						{
							Assert.AreEqual(expectedStream_AnnexII, outputStream_AnnexII);
							Assert.AreEqual(expectedStream_AnnexIV, outputStream_AnnexIV);
							Assert.AreEqual(expectedStream_AnnexV, outputStream_AnnexV);
							Assert.AreEqual(expectedStream_AnnexVI, outputStream_AnnexVI);
							Assert.AreEqual(expectedStream_AnnexVII, outputStream_AnnexVII);
						}
					}
				}
			}
		}
	}
}
