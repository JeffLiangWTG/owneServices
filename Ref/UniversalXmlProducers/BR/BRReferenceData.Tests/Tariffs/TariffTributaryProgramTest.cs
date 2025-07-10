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
	class TariffTributaryProgramTest
	{
		protected MockHttpMessageHandler MockHttp(Stream streamFile)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_PORTAL_UNICO_TRATAMENTOS_TRIBUTARIOS_DOWNLOAD"]).Respond("application/html", streamFile);

			CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

			return mockHttp;
		}

		[Test]
		public void TestListHasUnknownCountryBlocksName()
		{
			using (var program = new TariffTributaryProgramForTesting())
			using (StringWriter sw = new StringWriter())
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ttce-importacao-dados-para-servico-duimp-atualizado-20210630_Unknown.zip"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				var exception = Assert.Throws<InvalidOperationException>(() => { program.Run(); });
				Assert.That(exception.Message, Does.Contain("Unknown block name founded (UNIÃO EUROPÉIA - UE)"));
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new TariffTributaryProgramForTesting())
			using (StringWriter sw = new StringWriter())
			using (var streamSame = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ttce-importacao-dados-para-servico-duimp-atualizado-20210630.zip"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.ttce-importacao-dados-para-servico-duimp-atualizado-20210630.zip"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });

				mock = MockHttp(streamSame);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Tariff Tributary TCCE, exported with success!\r\nNo update found on Tariff Tributary TCCE!"));
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

		class TariffTributaryProgramForTesting : TariffTributaryProgram, IDisposable
		{
			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
