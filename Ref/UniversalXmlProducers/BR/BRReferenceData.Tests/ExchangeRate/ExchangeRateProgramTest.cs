using System;
using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class ExchangeRateProgramTest
	{
		[Test]
		public void TestRun_HandleEmptyData()
		{
			using (var csvStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Output.cotacaoTodasAsMoedas_10052021.csv"))
			using (var htmlStream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.ExchangeRate.TestFiles.Input.consultaBoletim.html"))
			{
				var today = new DateTime(2022, 6, 7);
				var mockHttp = new MockHttpMessageHandler();
				using (var response = new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = null,
				})
				{
					void mockRespondForDate(DateTime date, bool emptyContent)
					{
						var url = ConfigurationProvider.Configuration["BRExchangeRateUrl"] + $"&RadOpcao=2&ChkMoeda=61&DATAINI={date:dd}%2F{date:MM}%2F{date:yyyy}&DATAFIM=";
						if (emptyContent)
						{
							mockHttp.When(HttpMethod.Get, url).Respond(req => response);
						}
						else
						{
							mockHttp.When(HttpMethod.Get, url).Respond("application/html", htmlStream);
						}
					}

					mockRespondForDate(new DateTime(2022, 6, 1), false);
					mockRespondForDate(new DateTime(2022, 6, 2), false);
					mockRespondForDate(new DateTime(2022, 6, 3), true);
					mockRespondForDate(new DateTime(2022, 6, 6), true);

					mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["BRExchangeRateUrlCsv"] + "&id=61682").Respond("application/csv", csvStream);

					using (var program = new BRExchangeRateProgramForTesting())
					{
						program.Mock = mockHttp;
						program.DeleteLogFile();
						var consoleOuput = RunExchangeRateProgram(program, today);
						Assert.AreEqual(@"Download Exchange Rate for 2022-06-01
Download Exchange Rate for 2022-06-02
Download Exchange Rate for 2022-06-03
Failed to download exchange rate data for 2022-06-03...1
Failed to download exchange rate data for 2022-06-03...2
Failed to download exchange rate data for 2022-06-03...3
Download Exchange Rate for 2022-06-06
Failed to download exchange rate data for 2022-06-06...1
Failed to download exchange rate data for 2022-06-06...2
Failed to download exchange rate data for 2022-06-06...3
", consoleOuput, "Download from 2022-06-01 to 2022-06-06");

						Assert.IsTrue(File.Exists(program.GetOutputFilePathForTesting("RefExchangeRateZZ_BR_CUE_20220601.xml")));
						Assert.IsTrue(File.Exists(program.GetOutputFilePathForTesting("RefExchangeRateZZ_BR_CUS_20220601.xml")));
						Assert.IsTrue(File.Exists(program.GetOutputFilePathForTesting("RefExchangeRateZZ_BR_CUE_20220602.xml")));
						Assert.IsTrue(File.Exists(program.GetOutputFilePathForTesting("RefExchangeRateZZ_BR_CUS_20220602.xml")));

						Assert.IsTrue(File.Exists(program.LogFilePath));
						Assert.AreEqual("2022-06-02", File.ReadAllText(program.LogFilePath));

						Assert.AreEqual("", RunExchangeRateProgram(program, new DateTime(2022, 6, 3)));

						consoleOuput = RunExchangeRateProgram(program, today);
						Assert.AreEqual(@"Download Exchange Rate for 2022-06-03
Failed to download exchange rate data for 2022-06-03...1
Failed to download exchange rate data for 2022-06-03...2
Failed to download exchange rate data for 2022-06-03...3
Download Exchange Rate for 2022-06-06
Failed to download exchange rate data for 2022-06-06...1
Failed to download exchange rate data for 2022-06-06...2
Failed to download exchange rate data for 2022-06-06...3
", consoleOuput, "Download from 2022-06-03 to 2022-06-06");
					}
				}
			}
		}

		string RunExchangeRateProgram(BRExchangeRateProgramForTesting program, DateTime date)
		{
			using (var logStream = new MemoryStream())
			using (var logWriter = new StreamWriter(logStream))
			{
				logWriter.AutoFlush = true;
				Console.SetOut(logWriter);

				program.Today = date;
				program.Run();

				using (var strRead = new StreamReader(logStream))
				{
					logStream.Position = 0;
					return strRead.ReadToEnd();
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

		class BRExchangeRateProgramForTesting : ExchangeRateProgram, IDisposable
		{
			public MockHttpMessageHandler Mock { get; set; }

			protected override HttpClient GetHttpClient() => Mock.ToHttpClient();

			protected override int SleepInterval => 1;

			public DateTime Today { get; set; }

			public string GetOutputFilePathForTesting(string fileName) => GetOutputFilePath(fileName);

			protected override DateTime GetToday() => Today;

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
