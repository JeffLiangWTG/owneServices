using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business.ExchangeRate;
using CargoWise.RefDbRepo.AUReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate
{
	[TestFixture]
	[SetCulture("en-AU")]
	public class ExchangeRateProgramTest
	{
		[Test, Timeout(10000)]
		public void TestSuccess()
		{
			string outputFilePath = null;

			try
			{
				using (var monitor = new ConsoleOutputMonitor())
				{
					var webPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/";
					var program = new MockExchangeRateProgramFixedConfig()
					{
						WebPageUrl = webPageUrl
					};

					using (var streamHtml = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles._reference_production_main_.html"))
					using (var streamTxt = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles.XCHGRATE-P1-EDMAIN-2002220144.txt"))					
					using (var readerHtml = new StreamReader(streamHtml, Encoding.UTF8))
					using (var readerTxt = new StreamReader(streamTxt, Encoding.UTF8))
					{
						program.MockParser.MockHttpClientHelper.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/")).Returns(Task.FromResult(readerHtml.ReadToEnd()));
						program.MockParser.MockHttpClientHelper.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/XCHGRATE-P1-EDMAIN-2002220144.txt")).Returns(Task.FromResult(readerTxt.ReadToEnd()));

						var outputPath = ExchangeRateTestHelper.TestFilesPath;
						outputFilePath = Path.Combine(outputPath, program.OutputFileName);
						program.Run(outputPath);

						string expectedConsoleOutput = "ExchangeRate Parser completed successfully.\r\n";
						Assert.AreEqual(expectedConsoleOutput, monitor.ToString());
					}
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(outputFilePath))
				{
					File.Delete(outputFilePath);
				}
			}
		}

		[Test, Timeout(10000)]
		public void TestRetryOnFailure()
		{
			using (var monitor = new ConsoleOutputMonitor())
			{
				var badWebPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/" + "XXX" + "_reference_production_main_.html";
				var program = new MockExchangeRateProgramFixedConfig()
				{
					WebPageUrl = badWebPageUrl
				};
				var outputPath = ExchangeRateTestHelper.TestFilesPath;
				program.Run(outputPath);

				string expectedConsoleOutput = $@"#1 ExchangeRate Parser encountered the following errors:	Error Retrieving Source from {badWebPageUrl}: Could not retrieve any content.
ExchangeRate Parser will run again in 1 seconds.	
#2 ExchangeRate Parser encountered the following errors:	Error Retrieving Source from {badWebPageUrl}: Could not retrieve any content.
ExchangeRate Parser will run again in 1 seconds.	
#3 ExchangeRate Parser encountered the following errors:	Error Retrieving Source from {badWebPageUrl}: Could not retrieve any content.
";

				Assert.AreEqual(expectedConsoleOutput, monitor.ToString());
			}
		}

		[Test, Timeout(10000)]
		public void TestRetryOnFailureObeysConfig()
		{
			var propertyDict = new Dictionary<string, string>
			{
				{ "ExchangeRateRetryDelay", "2000" },
				{ "ExchangeRateMaxAttempts", "2" }
			};
			ApplicationConfigTests.ChangeConfigTemprory4Test(propertyDict, () =>
			{
				using (var monitor = new ConsoleOutputMonitor())
				{
					var badWebPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/" + "XXX" + "_reference_production_main_.html";
					var program = new MockExchangeRateProgram()
					{
						WebPageUrl = badWebPageUrl
					};

					var outputPath = ExchangeRateTestHelper.TestFilesPath;
					program.Run(outputPath);

					string expectedConsoleOutput = $@"#1 ExchangeRate Parser encountered the following errors:	Error Retrieving Source from {badWebPageUrl}: Could not retrieve any content.
ExchangeRate Parser will run again in 2 seconds.	
#2 ExchangeRate Parser encountered the following errors:	Error Retrieving Source from {badWebPageUrl}: Could not retrieve any content.
";

					Assert.AreEqual(expectedConsoleOutput, monitor.ToString());
				}
			});
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}

	#region Implementation

	public class MockExchangeRateProgram : ExchangeRateProgram
	{
		public string WebPageUrl
		{
			get;
			set;
		}

		public string OutputFileName => MockParser.OutputFileName;

		protected override XCHAGRATEParser GetParser() => MockParser;

		public MockXCHAGRATEParser MockParser => parser ?? (parser = new MockXCHAGRATEParser() { WebPageUrlForTest = WebPageUrl });
		MockXCHAGRATEParser parser;
	}

	public class MockExchangeRateProgramFixedConfig : MockExchangeRateProgram
	{
		protected override int RetryDelay => 1000;
		protected override int MaxAttempts => 3;
	}

	class ConsoleOutputMonitor : StringWriter
	{
		public ConsoleOutputMonitor()
		{
			originalOutput = Console.Out;
			originalError = Console.Error;

			Console.SetOut(this);
			Console.SetError(this);
		}

		protected override void Dispose(bool disposing)
		{
			Console.SetOut(originalOutput);
			Console.SetError(originalError);
			base.Dispose(disposing);
		}

		readonly TextWriter originalOutput;
		readonly TextWriter originalError;
	}

	#endregion
}
