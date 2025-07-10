using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using FlexCel.XlsAdapter;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffTableNCMTest
	{
		public void TestDownloadTableNCMNoMock()
		{
			using (var client = new HttpClient())
			{
				var downloader = new TariffTableNCMDownloader();
				var fileNcm = downloader.DownloadTableNCM(client);

				Assert.NotNull(fileNcm, nameof(fileNcm));

				using (var sXlsx = new MemoryStream(fileNcm))
				{
					var xls = new XlsFile(sXlsx, true);

					Assert.NotNull(xls, nameof(xls));

					xls?.SetSheetSelected(1, true);

					Assert.True(xls?.GetCellValue(1, 2)?.ToString().Contains("Início de vigência da NCM  no Siscomex"));
					Assert.True(xls?.GetCellValue(1, 3)?.ToString().Contains("Fim de vigência da NCM no Siscomex"));
					Assert.True(xls?.GetCellValue(1, 4)?.ToString().Contains("uTrib para uso em operações de Exportação (Abreviatura)"));
					Assert.True(xls?.GetCellValue(1, 5)?.ToString().Contains("Descrição da uTrib utilizada em operações de Exportação"));
				}
			}
		}

		[Test]
		public void TestDownloadTableNCM()
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_NOTA_FISCAL_ELETRONICA"]).Respond("application/html",
					Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NotaFiscalEletronica.File_Nota_Fiscal_Eletronica.html"));

				var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NotaFiscalEletronica.Table_NCM.xlsx");
				mockHttp.When("https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=we6y2AfzKCo=").Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream);

				using (var client = mockHttp.ToHttpClient())
				{
					var downloader = new TariffTableNCMDownloader();
					var fileNcm = downloader.DownloadTableNCM(client);

					Assert.NotNull(fileNcm, nameof(fileNcm));

					using (var sXlsx = new MemoryStream(fileNcm))
					{
						var xls = new XlsFile(sXlsx, true);

						Assert.NotNull(xls, nameof(xls));

						xls?.SetSheetSelected(1, true);

						Assert.True(xls?.GetCellValue(1, 2)?.ToString().Contains("Início de vigência da NCM  no Siscomex"));
						Assert.True(xls?.GetCellValue(1, 3)?.ToString().Contains("Fim de vigência da NCM no Siscomex"));
						Assert.True(xls?.GetCellValue(1, 4)?.ToString().Contains("uTrib para uso em operações de Exportação (Abreviatura)"));
						Assert.True(xls?.GetCellValue(1, 5)?.ToString().Contains("Descrição da uTrib utilizada em operações de Exportação"));
					}
				}
			}
		}
	}
}
