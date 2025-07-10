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
	public class TariffBitAndBKDownloadProgramTest
	{
		[Test]
		public void TestNoChangeFound()
		{
			using (var program = new BRTariffBitAndBKDownloadProgramForTesting())
			using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.BIT_AND_BK.BIT_AND_BK_2021_11_29.json"))
			{
				program.DeleteLogFile();

				var mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_BIT_BK"]).Respond("Application/file", stream);
				RunMock(program, mock, "File Bit and BK Json file, exported with success!\r\n");
				RunMock(program, mock, "No update found on Bit and BK Json file!\r\n");
			}
		}

		[Test]
		public void TestChangeFound()
		{
			using (var program = new BRTariffBitAndBKDownloadProgramForTesting())
			using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.BIT_AND_BK.BIT_AND_BK_2021_11_29.json"))
			using (var updatedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.BIT_AND_BK.BIT_AND_BK_2021_11_30.json"))
			{
				program.DeleteLogFile();

				var mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_BIT_BK"]).Respond("Application/file", stream);
				RunMock(program, mock, "File Bit and BK Json file, exported with success!\r\n");

				mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_BIT_BK"]).Respond("Application/file", updatedStream);
				RunMock(program, mock, "File Bit and BK Json file, exported with success!\r\n");
			}
		}

		void RunMock(BRTariffBitAndBKDownloadProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
		{
			using (StringWriter sw = new StringWriter())
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

		class BRTariffBitAndBKDownloadProgramForTesting : TariffBitAndBKDownloadProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }
			protected override HttpClient GetHttpClientWithHandler(HttpClientHandler handler) => mock?.ToHttpClient();

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
