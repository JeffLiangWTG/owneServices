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
	public class WTOIIITariffProgramTest
	{
		[Test]
		public void TestOMCFileDownload()
		{
			using (var file = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex.xlsx"))
			{
				using (var mock = new MockHttpMessageHandler())
				{
					mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_OMC_TEC"]).Respond("Application/file", file);

					using (var client = mock.ToHttpClient())
					{
						var bytesFile = RateOMCTecDownloader.DownloadOMCTecFile(client);
						using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex.xlsx"))
						{
							Assert.IsTrue(CompareUtils.AreEquals(bytesFile, StreamUtils.ConvertStreamToByteArray(expectedStream)));
						}
					}
				}
			}
		}

		[Test]
		public void TestWhenOMCTecLogIsNull()
		{
			using (var program = new BRTariffWTOProgramForTesting())
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex.xlsx"))
			{
				Assert.IsFalse(program.DataSourceHasUpdate(null));
			}
		}

		[Test]
		public void TestWhenOMCTecLogIsEquals()
		{
			using (var program = new BRTariffWTOProgramForTesting())
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex.xlsx"))
			{
				var bFile = StreamUtils.ConvertStreamToByteArray(expectedStream);
				Assert.IsTrue(program.DataSourceHasUpdate(bFile));

				program.UpdateLogFile(bFile);
				Assert.IsFalse(program.DataSourceHasUpdate(bFile));
			}
		}

		[Test]
		public void TestWhenOMCTecLogIsNotEquals()
		{
			using (var program = new BRTariffWTOProgramForTesting())
			using (var expectedStreamUpdated = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex_updated.xlsx"))
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex.xlsx"))
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
			using (var program = new BRTariffWTOProgramForTesting())
			using (var expectedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex.xlsx"))
			using (var expectedStreamUpdated = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.omc_perfuracoes_tec_com_ex_updated.xlsx"))
			{
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);

					var mock = new MockHttpMessageHandler();
					mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_OMC_TEC"]).Respond("Application/file", expectedStream);

					program.MockHttpMessageHandler = mock;
					program.DeleteLogFile();

					Assert.DoesNotThrow(() => program.Run(), "First Run, update when no log file exists");
					Assert.That(sw.ToString(), Does.Contain("File OMC Tec file for WTO, exported with success!"));
				}

				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					Assert.DoesNotThrow(() => program.Run(), "Second Run, no update when content in log file equals to downloaded content");
					Assert.That(sw.ToString(), Does.Contain("No update found on OMC Tec file for WTO!"));
				}

				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					var mockUpdated = new MockHttpMessageHandler();
					mockUpdated.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_OMC_TEC"]).Respond("Application/file", expectedStreamUpdated);

					program.MockHttpMessageHandler = mockUpdated;

					Assert.DoesNotThrow(() => program.Run(), "Third Run, has update when content in log file not equals to downloaded content");
					Assert.That(sw.ToString(), Does.Contain("File OMC Tec file for WTO, exported with success!"));
				}
			}
		}

		class BRTariffWTOProgramForTesting : WTOIIITariffProgram, IDisposable
		{
			public BRTariffWTOProgramForTesting() : base()
			{
			}

			public MockHttpMessageHandler MockHttpMessageHandler;

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null)
			{
				return MockHttpMessageHandler.ToHttpClient();
			}

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
