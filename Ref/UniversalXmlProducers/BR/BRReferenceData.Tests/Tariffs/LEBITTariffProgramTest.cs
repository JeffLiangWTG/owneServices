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
	public class LEBITTariffProgramTest
	{
		[Test]
		public void TestListHasBeenUpdated()
		{
			using (var program = new BRTariffLebitListProgramForTesting())
			using (var updatedStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk_updated.xlsx"))
			using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk.xlsx"))
			{
				program.DeleteLogFile();

				var mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LEBIT_LIST"]).Respond("Application/file", stream);
				RunMock(program, mock, "File LEBIT file, exported with success!");

				mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LEBIT_LIST"]).Respond("Application/file", updatedStream);
				RunMock(program, mock, "No update found on LEBIT file!", false);
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new BRTariffLebitListProgramForTesting())
			using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk.xlsx"))
			using (var sameStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.LEBIT.anexo_vi_lebit_bk.xlsx"))
			{
				program.DeleteLogFile();

				var mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LEBIT_LIST"]).Respond("Application/file", stream);
				RunMock(program, mock, "File LEBIT file, exported with success!");

				mock = new MockHttpMessageHandler();
				mock.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_LEBIT_LIST"]).Respond("Application/file", sameStream);
				RunMock(program, mock, "No update found on LEBIT file!");
			}
		}

		void RunMock(BRTariffLebitListProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
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

		class BRTariffLebitListProgramForTesting : LEBITTariffProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();
		}
	}
}
