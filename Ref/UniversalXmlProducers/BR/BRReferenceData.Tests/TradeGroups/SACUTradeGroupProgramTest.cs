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
	class SACUTradeGroupProgramTest
	{
		protected MockHttpMessageHandler MockHttp(string htmlString)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_SACU_AGREEMENTS"]).Respond("application/html", Utils.GetManifestResourceStream(htmlString));

			CaptchaSolvingUtils.
			Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

			return mockHttp;
		}

		[Test]
		public void TestListHasBeenUpdated()
		{

			var htmlStringUpdated = $"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACU_TRADE_GROUP_GET_UPDATED.html";
			var htmlString = $"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACU_TRADE_GROUP_GET.html";
			using (var program = new BRSACUTradeGroupProgramForTesting())
			using (StringWriter sw = new StringWriter())
			{
				Console.SetOut(sw);

				var mock = MockHttp(htmlString);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File SACU Trade Group, exported with success!"));

				mock = MockHttp(htmlStringUpdated);
				program.mock = mock;

				var exception = Assert.Throws<InvalidOperationException>(() => { program.Run(); });
				Assert.IsTrue(exception.ToString().Contains("Country not found in countries table, please notify the brazilian team!"));
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			var htmlStringSame = $"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACU_TRADE_GROUP_GET.html";
			var htmlString = $"CargoWise.RefDbRepo.BRReferenceData.Tests.TradeGroups.TestFiles.Input.SACU_TRADE_GROUP_GET.html";
			using (var program = new BRSACUTradeGroupProgramForTesting())
			using (StringWriter sw = new StringWriter())
			{
				Console.SetOut(sw);

				var mock = MockHttp(htmlString);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File SACU Trade Group, exported with success!"));

				mock = MockHttp(htmlStringSame);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("No update found on SACU Trade Group!"));
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

		class BRSACUTradeGroupProgramForTesting : SACUTradeGroupProgram, IDisposable
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
