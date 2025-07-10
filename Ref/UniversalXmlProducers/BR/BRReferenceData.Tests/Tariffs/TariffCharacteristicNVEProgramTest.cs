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
	class TariffCharacteristicNVEProgramTest
	{
		protected MockHttpMessageHandler MockHttp(Stream streamFile)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NVE_ATTRIBUTE"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.NVE_GET.html"));
			mockHttp.When(HttpMethod.Post, ConfigurationProvider.Configuration["URL_TARIFF_NVE_ATTRIBUTE"]).Respond("application/html", streamFile);

			CaptchaSolvingUtils.
			Instance.SetResultToGetLoginResponse(() => new HttpResponseMessage() { StatusCode = HttpStatusCode.OK });

			return mockHttp;
		}

		[Test]
		public void TestListHasBeenUpdated()
		{
			using (var program = new BRRefCusTariffBRCharacteristicNveProgramForTesting())
			using (StringWriter sw = new StringWriter())
			using (var streamUpdated = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve_updated.xml"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve.xml"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Customs Tariff NVE Attribute file, exported with success!"));

				mock = MockHttp(streamUpdated);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.IsFalse(sw.ToString().Contains("No update found on Customs Tariff NVE Attribute file!"));
			}
		}

		[Test]
		public void TestListHasNotBeenUpdated()
		{
			using (var program = new BRRefCusTariffBRCharacteristicNveProgramForTesting())
			using (StringWriter sw = new StringWriter())
			using (var streamSame = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve.xml"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve.xml"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Customs Tariff NVE Attribute file, exported with success!"));

				mock = MockHttp(streamSame);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("No update found on Customs Tariff NVE Attribute file!"));
			}
		}

		[Test]
		public void TestLegalActWithMultipleNumbersOrStartDate()
		{
			using (var program = new BRRefCusTariffBRCharacteristicNveProgramForTesting())
			using (StringWriter sw = new StringWriter())
			using (var streamMultiple = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve_multiple.xml"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve_updated.xml"))
			using (var streamComplete = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.NVE.Nve.xml"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(stream);
				program.mock = mock;
				program.DeleteLogFile();

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Customs Tariff NVE Attribute file, exported with success!"));

				mock = MockHttp(streamComplete);
				program.mock = mock;

				Assert.DoesNotThrow(() => { program.Run(); });
				Assert.That(sw.ToString(), Does.Contain("File Customs Tariff NVE Attribute file, exported with success!"));

				mock = MockHttp(streamMultiple);
				program.mock = mock;

				var ex = Assert.Throws<InvalidOperationException>(() => program.Run());
				Assert.That(ex?.Message, Does.Contain("Multiple Legal act number founded for the same combination of (codigoNcm, codigoAtributo and codigoEspecificacao) (23040090, AA and 0001) legal Act Numbers ( 000082, 000081 )"));
				Assert.That(ex?.Message, Does.Contain("Generated Legal act of (codigoNcm, codigoAtributo, codigoEspecificacao and descricaoAtributo) (15079011, AC, 0002 and ACONDICIONAMENTO) is not using the minor date founded on the xml minor date: 01/07/2022, actual date: 01/07/2023"));
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

		class BRRefCusTariffBRCharacteristicNveProgramForTesting : TariffCharacteristicNVEProgram, IDisposable
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
