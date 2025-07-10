using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using static CargoWise.RefDbRepo.BRReferenceData.Tests.TariffRatesManagerTest;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	sealed class TariffRatesProgramTest : BaseTariffRatesTest
	{
		[Test]
		public void TestListHasBeenUpdated()
		{
			using (var program = new TariffRatesProgramForTesting())
			{
				program.DeleteLogFile();

				RunMock(program, GetMockedHttpClient(), "File BR Customs Tariff Rates file, exported with success!");
				RunMock(program, GetMockedHttpClient(true), "No update found on BR Customs Tariff Rates file!", false);
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new TariffRatesProgramForTesting())
			{
				program.DeleteLogFile();

				RunMock(program, GetMockedHttpClient(), "File BR Customs Tariff Rates file, exported with success!");
				RunMock(program, GetMockedHttpClient(), "No update found on BR Customs Tariff Rates file!");
			}
		}

		void RunMock(TariffRatesProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
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

		public class TariffRatesProgramForTesting : TariffRatesProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			protected override TariffRatesManager TariffRatesManager => new TariffRatesManagerForTesting();

			public string GetLogPath => LogFilePath;

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
