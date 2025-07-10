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
	class AutoPartsTariffProgramTest
	{
		protected MockHttpMessageHandler MockHttp(Stream streamFile)
		{
			var mockHttp = new MockHttpMessageHandler();
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_AUTO_PARTS_LIST"]).Respond("application/html", Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.AUTO_PART_LIST_GET.html"));
			mockHttp.When(HttpMethod.Get, $"https://www.gov.br/produtividade-e-comercio-exterior/pt-br/assuntos/competitividade-industrial/setor-automotivo/regime-autopecas/documentos-regime-de-autopecas/ListaGeraldeAutopeasNoProduzidasv24032022.xlsx").Respond("application/xlsx", streamFile);
			return mockHttp;
		}

		[Test]
		public void TestAutoPartsListHasChanges()
		{
			using (var program = new BRTariffAutoPartsListProgramForTesting())
			using (var updatedStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022_updated.xlsx"))
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022.xlsx"))
			{
				program.DeleteLogFile();
				RunMock(program, MockHttp(stream), "File Auto Parts List file, exported with success!");
				RunMock(program, MockHttp(updatedStream), "No update found on Auto Parts List file!", false);
			}
		}

		[Test]
		public void TestAutoPartsListHasNotChanged()
		{
			using (var program = new BRTariffAutoPartsListProgramForTesting())
			using (var stream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022.xlsx"))
			using (var streamSame = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Input.AutoPartsList.ListaGeraldeAutopeasNoProduzidasv24032022.xlsx"))
			{
				program.DeleteLogFile();
				RunMock(program, MockHttp(stream), "File Auto Parts List file, exported with success!");
				RunMock(program, MockHttp(streamSame), "No update found on Auto Parts List file!");
			}
		}

		void RunMock(BRTariffAutoPartsListProgramForTesting program, MockHttpMessageHandler mock, string message, bool messageShouldBeEquals = true)
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

		class BRTariffAutoPartsListProgramForTesting : AutoPartsTariffProgram, IDisposable
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
