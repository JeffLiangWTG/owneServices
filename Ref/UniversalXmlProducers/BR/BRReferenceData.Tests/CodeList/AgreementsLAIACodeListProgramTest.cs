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
	class AgreementsLAIACodeListProgramTest
	{
		protected MockHttpMessageHandler MockHttp(Stream streamFile)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_ACORDO_ALADI"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.ACORDO_ALADI_GET.html"));
			mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_ACORDO_ALADI"]).Respond("application/html", streamFile);
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELA_ACORDOS_TARIFARIOS"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.TabelaAcordosTarifarios.html"));

			CaptchaSolvingUtils.
			Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

			return mockHttp;
		}

		[Test]
		public void TestListHasBeenUpdated()
		{
			using (var program = new AgreementsLAIACodeListProgramForTesting())
			using (var sw = new StringWriter())
			using (var streamUpdated = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi_updated.xml"))
			using (var stream = Utils.GetManifestResourceStream("CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi.xml"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Duty LAIA Agreement Codes file, exported with success!"));

				mock = MockHttp(streamUpdated);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.IsFalse(sw.ToString().Contains("No update found on Duty LAIA Agreement Codes file!"));
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new AgreementsLAIACodeListProgramForTesting())
			using (var sw = new StringWriter())
			using (var streamSame = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi.xml"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.AcordoAladi.xml"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Duty LAIA Agreement Codes file, exported with success!"));

				mock = MockHttp(streamSame);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("No update found on Duty LAIA Agreement Codes file!"));
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

		class AgreementsLAIACodeListProgramForTesting : AgreementsLAIACodeListProgram, IDisposable
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
