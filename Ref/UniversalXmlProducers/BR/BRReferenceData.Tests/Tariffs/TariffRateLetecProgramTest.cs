using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffRateLetecProgramTest
	{
		protected MockHttpMessageHandler MockHttp(Stream streamFile)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_CURRENT_LISTS_TARIFF_RATES"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.LISTAS_VIGENTES_GET.html"));
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/camex/estrategia-comercial/arquivos-listas/anexo_v_letec.xlsx").Respond("application/xlsx", streamFile);
			return mockHttp;
		}

		[Test]
		public void TestRateLetecHasChanges()
		{
			using (var program = new BRTariffRateLetecProgramForTesting())
			{
				program.DeleteLogFile();

				using (var updatedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec_updated.xlsx"))
				using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
				{
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = MockHttp(stream);
						program.mock = mock;

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("File Rate Letec file, exported with success!\r\n", sw.ToString());
					}
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = MockHttp(updatedStream);
						program.mock = mock;

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("File Rate Letec file, exported with success!\r\n", sw.ToString());
					}
				}
			}
		}

		[Test]
		public void TestRateLetecHasNotChanged()
		{
			using (var program = new BRTariffRateLetecProgramForTesting())
			{
				program.DeleteLogFile();

				using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
				using (var streamSame = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.CustomsAllTariffRates.anexo_v_letec.xlsx"))
				{
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = MockHttp(stream);
						program.mock = mock;

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("File Rate Letec file, exported with success!\r\n", sw.ToString());
					}
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = MockHttp(streamSame);
						program.mock = mock;

						Assert.DoesNotThrow(() => { program.Run(); });
						Assert.AreEqual("No update found on Rate Letec file!\r\n", sw.ToString());
					}
				}
			}
		}

		class BRTariffRateLetecProgramForTesting : TariffRateLetecProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			protected override void ExportToXMLFile(byte[] bFile)
			{
			}

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
