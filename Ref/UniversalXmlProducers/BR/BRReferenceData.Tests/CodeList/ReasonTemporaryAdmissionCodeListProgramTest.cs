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
	class ReasonTemporaryAdmissionCodeListProgramTest
	{
		protected MockHttpMessageHandler MockHttp(Stream streamFile)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MOTIVO_ADMISSAO_TEMPORARIA"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MOTIVO_ADMISSAO_TEMPORARIA_GET.html"));
			mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TABELAS_ADUANEIRAS_DOWNLOAD_MOTIVO_ADMISSAO_TEMPORARIA"]).Respond("application/html", streamFile);

			CaptchaSolvingUtils.Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

			return mockHttp;
		}

		[Test]
		public void TestListHasBeenUpdated()
		{
			using (var program = new BRReasonTemporaryAdmissionProgramForTesting())
			using (var streamUpdated = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MotivoAdmissaoTemporaria_updated.xml"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MotivoAdmissaoTemporaria.xml"))
			{
				program.DeleteLogFile();
				RunMock(program, MockHttp(stream), "File Reason Temporary Admission file, exported with success!");
				RunMock(program, MockHttp(streamUpdated), "No update found on Reason Temporary Admission file!", false);
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new BRReasonTemporaryAdmissionProgramForTesting())
			using (var streamSame = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MotivoAdmissaoTemporaria.xml"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.CodeList.TestFiles.Input.MotivoAdmissaoTemporaria.xml"))
			{
				program.DeleteLogFile();
				RunMock(program, MockHttp(stream), "File Reason Temporary Admission file, exported with success!");
				RunMock(program, MockHttp(streamSame), "No update found on Reason Temporary Admission file!");
			}
		}

		void RunMock(BRReasonTemporaryAdmissionProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
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

		class BRReasonTemporaryAdmissionProgramForTesting : ReasonTemporaryAdmissionCodeListProgram, IDisposable
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
