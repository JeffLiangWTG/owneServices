using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffIPITableTest
	{
		[Test]
		public void TestIPITableDownload()
		{
			using (var file = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.IPI_TABLE.tabela-regimes-de-tributacao-ipi.xlsx"))
			{
				using (var mock = new MockHttpMessageHandler())
				{
					mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_IPI_TABLE"]).Respond("Application/file", file);

					using (var client = mock.ToHttpClient())
					{
						var bytesFile = TariffIPITableDownloader.Download(client);
						using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.IPI_TABLE.tabela-regimes-de-tributacao-ipi.xlsx"))
						{
							Assert.IsTrue(CompareUtils.AreEquals(bytesFile, StreamUtils.ConvertStreamToByteArray(expectedStream)));
						}
					}
				}
			}
		}

		[Test]
		public void CheckIfHasAnyChanges()
		{
			using (var program = new BRTariffIPITableProgramForTesting())
			using (var expectedStreamUpdated = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.IPI_TABLE.tabela-regimes-de-tributacao-ipi_updated.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.IPI_TABLE.tabela-regimes-de-tributacao-ipi.xlsx"))
			{
				var bFile = StreamUtils.ConvertStreamToByteArray(expectedStream);
				Assert.IsTrue(program.DataSourceHasUpdate(bFile));

				program.UpdateLogFile(bFile);
				Assert.IsTrue(program.DataSourceHasUpdate(StreamUtils.ConvertStreamToByteArray(expectedStreamUpdated)));
			}
		}

		[Test]
		public void TestRun()
		{
			using (var program = new BRTariffIPITableProgramForTesting())
			{
				program.DeleteLogFile();

				using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.IPI_TABLE.tabela-regimes-de-tributacao-ipi.xlsx"))
				{
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						var mock = new MockHttpMessageHandler();
						mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_IPI_TABLE"]).Respond("Application/file", expectedStream);

						program.MockHttpMessageHandler = mock;

						Assert.DoesNotThrow(() => program.Run(), "First Run, has update when no log file exists");
						Assert.AreEqual("File IPI Table File, exported with success!\r\n", sw.ToString());
					}
					using (var sw = new StringWriter())
					{
						Console.SetOut(sw);

						Assert.DoesNotThrow(() => program.Run(), "Second Run, no update when content in log file equals to downloaded content");
						Assert.AreEqual("No update found on IPI Table File!\r\n", sw.ToString());
					}
				}
				using (var expectedStreamUpdated = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.IPI_TABLE.tabela-regimes-de-tributacao-ipi_updated.xlsx"))
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					var mockUpdated = new MockHttpMessageHandler();
					mockUpdated.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_IPI_TABLE"]).Respond("Application/file", expectedStreamUpdated);

					program.MockHttpMessageHandler = mockUpdated;

					Assert.DoesNotThrow(() => program.Run(), "Third Run, has update when content in log file not equals to downloaded content");
					Assert.AreEqual("File IPI Table File, exported with success!\r\n", sw.ToString());
				}
			}
		}

		class BRTariffIPITableProgramForTesting : TariffIPITableProgram, IDisposable
		{
			public BRTariffIPITableProgramForTesting() : base()
			{
			}

			public MockHttpMessageHandler MockHttpMessageHandler;

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null)
			{
				return MockHttpMessageHandler.ToHttpClient();
			}

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
