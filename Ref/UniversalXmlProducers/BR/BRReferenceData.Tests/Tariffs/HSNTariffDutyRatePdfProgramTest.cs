using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using FlexCel.XlsAdapter;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class HSNTariffDutyRatePdfProgramTest
	{
		protected MockHttpMessageHandler MockHttp(List<Stream> streamFiles)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_CURRENT_LISTS_TARIFF_RATES"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.LISTAS_VIGENTES_GET.html"));
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/Anexo_II_Res_272_2021.xlsx").Respond("application/xlsx", streamFiles[0]);
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_iv_desabastecimento.xlsx").Respond("application/xlsx", streamFiles[1]);
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_v_letec.xlsx").Respond("application/xlsx", streamFiles[2]);
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_vi_lebit_bk.xlsx").Respond("application/xlsx", streamFiles[3]);
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_vii_lista_covid.xlsx").Respond("application/xlsx", streamFiles[4]);
			return mockHttp;
		}

		[Test]
		public void TestListHasBeenUpdated()
		{
			using (var program = new HSNTariffDutyRatePdfProgramForTesting())
			using (var stream_AnnexII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.Anexo_II_Res_272_2021.xlsx"))
			using (var stream_AnnexIV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_iv_desabastecimento.xlsx"))
			using (var stream_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			using (var stream_AnnexVI = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vi_lebit_bk.xlsx"))
			using (var stream_AnnexVII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vii_lista_covid.xlsx"))
			using (var updatedStream_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec_updated.xlsx"))
			{
				program.DeleteLogFile();

				var streamFiles = new List<Stream> { stream_AnnexII, stream_AnnexIV, stream_AnnexV, stream_AnnexVI, stream_AnnexVII };
				RunMock(program, MockHttp(streamFiles), "Tariff Vigent Rate files, exported with success!");

				streamFiles = new List<Stream> { stream_AnnexII, stream_AnnexIV, updatedStream_AnnexV, stream_AnnexVI, stream_AnnexVII };
				RunMock(program, MockHttp(streamFiles), "No update found on Tariff Vigent Rate files!", false);
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new HSNTariffDutyRatePdfProgramForTesting())
			using (var stream_AnnexII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.Anexo_II_Res_272_2021.xlsx"))
			using (var stream_AnnexIV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_iv_desabastecimento.xlsx"))
			using (var stream_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			using (var stream_AnnexVI = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vi_lebit_bk.xlsx"))
			using (var stream_AnnexVII = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_vii_lista_covid.xlsx"))
			using (var sameStream_AnnexV = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
			{
				program.DeleteLogFile();

				var streamFiles = new List<Stream> { stream_AnnexII, stream_AnnexIV, stream_AnnexV, stream_AnnexVI, stream_AnnexVII };
				RunMock(program, MockHttp(streamFiles), "Tariff Vigent Rate files, exported with success!");
				RunMock(program, MockHttp(streamFiles), "No update found on Tariff Vigent Rate files!");
			}
		}

		[Test]
		public void TestXlsxCleaning()
		{
			using (var program = new HSNTariffDutyRatePdfProgramForTesting())
			using (var streamAfter = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.xlsx_cleaning_test_file_after.xlsx"))
			using (var streamBefore = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.xlsx_cleaning_test_file_before.xlsx"))
			{
				var xls = new XlsFile(streamAfter, true);
				using (var output = new MemoryStream())
				{
					xls.Save(output, FlexCel.Core.TFileFormats.Xlsx);
					output.Position = 0;
					Assert.AreEqual(StreamUtils.ConvertStreamToByteArray(output), program.GetXlsFileCleaning(streamBefore));
				}
			}
		}

		void RunMock(HSNTariffDutyRatePdfProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				if (messageShouldBeEquals)
				{
					Assert.That(sw.ToString(), Does.Contain(message));
				}
				else
				{
					Assert.IsFalse(sw.ToString().Contains(message));
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			defOut = Console.Out;
		}

		[TearDown]
		public void TestCleanup()
		{
			Console.SetOut(defOut);
		}

		TextWriter defOut;

		public class HSNTariffDutyRatePdfProgramForTesting : HSNTariffDutyRatePdfProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			public string GetLogPath => LogFilePath;

			public byte[] GetXlsFileCleaning(Stream stream)
			{
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return XlsFileCleaning(ms.ToArray());
				}
			}

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
