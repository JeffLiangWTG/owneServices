using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{
	[TestFixture]
	sealed class ProcessManagerTests
	{
		[Test]
		public void RunProcess()
		{
			processManager.RunProcess(string.Empty, errorCollector);
			var builder = processManager.TestProcedureBuilder as VirtualProcedureBuilder;
			Assert.That(builder, Is.Not.Null);
			Assert.That(builder.ProcedureCodeDataCount, Is.EqualTo(957));
			Assert.That(builder.CategoryProcedureMappingDataCount, Is.EqualTo(8));
		}

		[Test]
		public void RunProcessFailed()
		{
			processManager.TestWebClientMock.Setup(x => x.GetContent(It.Is<string>(i => i == @"https://www.gov2.uk"))).Returns(string.Empty);
			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith($"Processing failed"), () => processManager.RunProcess(string.Empty, errorCollector));
		}

		void SetupProcessManager()
		{
			var importProcedure = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ImportProcedure.html");
			var importAdditionalProcedureCodes = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ImportAdditionalProcedureCodes.html");
			var importAdditionalProcedureCodesMatrixHtml = TestHelper.ReadManifestResourceContent($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.CorrelationMatrixDownloadPage.html");
			var importAdditionalProcedureCodesMatrixOds = TestHelper.ReadManifestResourceContentBytes($"CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ImportAdditionalProcedureCodesMatrix.ods");

			var configFile = Path.Combine(TempFolder, "TestConfigFile");
			TestHelper.SimulateDownload(configFile, "CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Input.ConfigFile_ValidSingle.xml");

			processManager = new ProcessManagerTester();
			processManager.TestConfigProviderMock.Setup(cp => cp.ConfigFile).Returns(configFile);
			processManager.TestWebClientMock.Setup(x => x.GetContent(It.Is<string>(i => i == @"https://www.gov1.uk"))).Returns(importProcedure);
			processManager.TestWebClientMock.Setup(x => x.GetContent(It.Is<string>(i => i == @"https://www.gov2.uk"))).Returns(importAdditionalProcedureCodes);
			processManager.TestWebClientMock.Setup(x => x.GetContent(It.Is<string>(i => i == @"https://www.gov3.uk"))).Returns(importAdditionalProcedureCodesMatrixHtml);
			processManager.TestWebClientMock.Setup(x => x.GetContentAsByteArray(@"https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/1100405/DE-1-10-to-1-11-correlation-matrix.ods")).Returns(importAdditionalProcedureCodesMatrixOds);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);

			SetupProcessManager();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			errorCollector = new StringBuilder();
		}

		string TempFolder;
		StringBuilder errorCollector;
		ProcessManagerTester processManager;
	}
}
