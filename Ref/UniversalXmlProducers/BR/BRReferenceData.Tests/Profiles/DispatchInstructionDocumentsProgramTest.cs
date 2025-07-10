using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	class DispatchInstructionDocumentsProgramTest
	{
		protected MockHttpMessageHandler MockHttp(bool isProduction, bool useUpdatedSource = false)
		{
			var mockHttp = new MockHttpMessageHandler();
			DocumentTypeOperationDownloaderTest.MockHttp(mockHttp, isProduction, Constants.TariffAttributes.OperationTypes.LPCO);
			DocumentTypeOperationDownloaderTest.MockHttp(mockHttp, isProduction, Constants.TariffAttributes.OperationTypes.DUIMP);
			DocumentTypeOperationDownloaderTest.MockHttp(mockHttp, isProduction, Constants.TariffAttributes.OperationTypes.CATP);

			if (useUpdatedSource)
			{
				AddMockHttp(mockHttp, isProduction, "103");
				AddMockHttp(mockHttp, isProduction, "3");
				AddMockHttp(mockHttp, isProduction, "2");
			}
			else
			{
				DocumentTypeDownloaderTest.MockHttp(mockHttp, isProduction, "103");
				DocumentTypeDownloaderTest.MockHttp(mockHttp, isProduction, "3");
				DocumentTypeDownloaderTest.MockHttp(mockHttp, isProduction, "2");
			}

			return mockHttp;
		}

		void AddMockHttp(MockHttpMessageHandler mockHttp, bool isProduction, string idDocument)
		{
			var updatedJsonPath = "CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentKeywords_updated.json";
			mockHttp.When(HttpMethod.Get, ConfigurationProvider.Configuration[DocumentTypeDownloaderTest.GetParameterURL(isProduction)].Replace("{idTipoDocumento}", idDocument)).Respond("Application/file", Utils.GetManifestResourceStream(updatedJsonPath));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TestListHasBeenUpdated(bool isProduction)
		{
			using (var sw = new StringWriter())
			using (var inputStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentType_result.txt"))
			using (var inputStreamUpdated = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentType_result_updated.txt"))
			{
				Console.SetOut(sw);
				var messageForTest = isProduction ? "" : " Test";

				using (var program = new DispatchInstructionDocumentsProgramForTesting(isProduction))
				{
					var mock = MockHttp(isProduction);
					program.mock = mock;
					program.DeleteLogFile();

					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.AreEqual(inputStream.StreamToString(), program.ReadLogFile());
					Assert.That(sw.ToString(), Does.Contain($"File BR Dispatch Instruction Documents{messageForTest}, exported with success!"));

					mock = MockHttp(isProduction, true);
					program.mock = mock;
					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.AreEqual(inputStreamUpdated.StreamToString(), program.ReadLogFile());
					Assert.IsFalse(sw.ToString().Contains($"No update found on Customs Tariff NCM {messageForTest}Attribute file!"));
				}
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void TesteListHasNotBeenUpdated(bool isProduction)
		{
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				var messageForTest = isProduction ? "" : " Test";

				using (var program = new DispatchInstructionDocumentsProgramForTesting(isProduction))
				{
					var mock = MockHttp(isProduction);
					program.mock = mock;
					program.DeleteLogFile();

					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.That(sw.ToString(), Does.Contain($"File BR Dispatch Instruction Documents{messageForTest}, exported with success!"));

					Assert.DoesNotThrow(() => { program.Run(); });
					Assert.That(sw.ToString(), Does.Contain($"No update found on BR Dispatch Instruction Documents{messageForTest}!"));
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			Console.SetOut(Console.Out);
		}

		class DispatchInstructionDocumentsProgramForTesting : DispatchInstructionDocumentsProgram, IDisposable
		{
			public DispatchInstructionDocumentsProgramForTesting(bool isProduction) : base(isProduction)
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
