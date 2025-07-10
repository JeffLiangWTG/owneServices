using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	sealed class TariffProfileNCMProgramTest
	{
		MockHttpMessageHandler MockHttp(Stream streamTariffs, Stream streamAttributes, bool isProduction, Stream versions)
		{
			var mockHttp = new MockHttpMessageHandler();
			if (isProduction)
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE"]).Respond("Application/file", streamTariffs);
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_ATTRIBUTE"]).Respond("Application/file", streamAttributes);
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_VERSION"]).Respond("Application/file", versions);
			}
			else
			{
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_TEST"]).Respond("Application/file", streamTariffs);
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_ATTRIBUTE_TEST"]).Respond("Application/file", streamAttributes);
				mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration["URL_TARIFF_NCM_ATTRIBUTE_VERSION_TEST"]).Respond("Application/file", versions);
			}
			return mockHttp;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValidatedListHasBeenUpdated(bool isProduction)
		{
			using (var sw = new StringWriter())
			using (var streamTariffs = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"))
			using (var streamAttributes = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_2023_10_11.zip"))
			using (var streamVersions = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.historico-versoes.html"))
			using (var streamVersionsUpdated = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.historico-versoes_updated.html"))
			{
				Console.SetOut(sw);
				var messageForTest = isProduction ? "" : " Test";

				using (var program = new TariffProfileNCMProgramForTesting(isProduction))
				{
					var mock = MockHttp(streamTariffs, streamAttributes, isProduction, streamVersions);
					program.mock = mock;
					program.DeleteLogFile();

					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.AreEqual("versao37", program.ReadLogFile());
					Assert.That(sw.ToString(), Does.Contain($"File BR Tariff Attributes{messageForTest}, exported with success!"));

					mock = MockHttp(streamTariffs, streamAttributes, isProduction, streamVersionsUpdated);
					program.mock = mock;
					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.AreEqual("versao38", program.ReadLogFile());
					Assert.IsFalse(sw.ToString().Contains($"No update found on BR Tariff Attributes{messageForTest}!"));
				}
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValidateThrowsExceptionStyleNotFound(bool isProduction)
		{
			using (var program = new TariffProfileNCMProgramForTesting(isProduction))
			using (var sw = new StringWriter())
			using (var streamTariffs = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"))
			using (var streamAttributes = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_2023_10_11_new_style.zip"))
			using (var streamVersions = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.historico-versoes.html"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(streamTariffs, streamAttributes, isProduction, streamVersions);
				program.mock = mock;
				program.DeleteLogFile();

				var ex = Assert.Throws<InvalidOperationException>(() => { program.Run(); });
				Assert.That(ex.Message, Does.Contain("Style not found: LISTA_ESTATICA2\r\n"));
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValidateListHasNotBeenUpdated(bool isProduction)
		{
			using (var sw = new StringWriter())
			using (var streamTariffs = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"))
			using (var streamAttributes = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_2023_10_11.zip"))
			using (var streamVersions = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.historico-versoes.html"))
			{
				Console.SetOut(sw);
				var messageForTest = isProduction ? "" : " Test";

				using (var program = new TariffProfileNCMProgramForTesting(isProduction))
				{
					var mock = MockHttp(streamTariffs, streamAttributes, isProduction, streamVersions);
					program.mock = mock;
					program.DeleteLogFile();

					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.That(sw.ToString(), Does.Contain($"File BR Tariff Attributes{messageForTest}, exported with success!"));

					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.IsFalse(sw.ToString().Contains($"No update found on BR Tariff Attributes{messageForTest}!"));
				}
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValidateThrowsExceptionFileWithoutDate(bool isProduction)
		{
			using (var program = new TariffProfileNCMProgramForTesting(isProduction))
			using (var sw = new StringWriter())
			using (var streamTariffs = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_POR_NCM_2023_10_11.zip"))
			using (var streamAttributes = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.ATRIBUTOS_WithoutDate.zip"))
			using (var streamVersions = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.NCM.historico-versoes.html"))
			{
				Console.SetOut(sw);

				var mock = MockHttp(streamTariffs, streamAttributes, isProduction, streamVersions);
				program.mock = mock;
				program.DeleteLogFile();

				var ex = Assert.Throws<InvalidOperationException>(() => { program.Run(); });
				Assert.That(ex.Message, Does.Contain("Filename 'ATRIBUTOS_WithoutDate/ATRIBUTOS_WithoutDate.json' doesn't contain a valid date."));
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			Console.SetOut(Console.Out);
		}

		class TariffProfileNCMProgramForTesting : TariffProfileNCMProgram, IDisposable
		{
			public TariffProfileNCMProgramForTesting(bool isProduction) : base(isProduction)
			{
			}

			public MockHttpMessageHandler mock { get; set; }

			protected override HttpClient GetHttpClient(HttpClientHandler handler = null) => mock?.ToHttpClient();

			public string ReadLogFile() => File.ReadAllText(LogFilePath);

			public void Dispose()
			{
				DeleteLogFile();
				DeleteOutputFiles();
			}
		}
	}
}
