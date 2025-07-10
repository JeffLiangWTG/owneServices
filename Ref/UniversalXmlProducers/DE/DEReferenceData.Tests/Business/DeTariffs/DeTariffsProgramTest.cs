using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.DEReferenceData.CmdLine;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs
{
	class DeTariffsProgramTest
	{
		[Test]
		public async Task DeTariffIntegration()
		{
			using var mockHttp = new MockHttpMessageHandler();

			mockHttp.When("https://shop.ezt-online.de/gdb/XD01296401_GDB.zip")
				.With(m =>
				{
					Assert.That(m.Headers.Authorization.Scheme, Is.EqualTo("Basic"));
					Assert.That(m.Headers.Authorization.Parameter, Is.EqualTo("Y2xpZW50Rm9yVGVzdDpzZWNyZXRGb3JUZXN0"));
					return true;
				})
				.Respond("application/zip",
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296401_GDB.zip"));

			mockHttp.When("https://shop.ezt-online.de/XD01296501.zip")
				.Respond("application/zip",
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296501.zip"));

			mockHttp.When("https://shop.ezt-online.de/XD01296602.zip")
				.Respond("application/zip",
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.XD01296602.zip"));

			mockHttp.When("https://shop.ezt-online.de/gdb/")
				.Respond("text/html",
					TestHelper.ReadManifestResourceContent(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.Index_gdb.html"));

			mockHttp.When("https://shop.ezt-online.de/")
				.Respond("text/html",
					TestHelper.ReadManifestResourceContent(
						"CargoWise.RefDbRepo.DEReferenceData.Tests.Services.DeTariffs.TestFiles.Index.html"));
			var de = new DeTariffsProgram() { HttpMessageHandler = mockHttp, };

			await de.ConvertTariffs(outputPath);

			mockHttp.VerifyNoOutstandingExpectation();

			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.DEReferenceData.Tests.Business.DeTariffs.TestFiles.Output.RefCusTariff_DE.xml");
			var actualUniversalXml = File.ReadAllText(Path.Combine(outputPath, "RefCusTariff_DE_IMP.xml"));
			Assert.That(actualUniversalXml, Is.EqualTo(expectedXML));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"DE\TestFiles\DeTariffsProgram\Output");
		}
		Assembly assembly;
		string outputPath;
	}
}
