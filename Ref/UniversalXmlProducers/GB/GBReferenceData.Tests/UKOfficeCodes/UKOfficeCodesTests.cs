using System.IO;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.UKOfficeCodes.Tests
{
	[TestFixture]
	sealed class UKOfficeCodesTests
	{
		[Test]
		public void RunProcess()
		{
			var odsData = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodes.ods");
			parser = new UKOfficeCodesParserForTest(errorCollector, odsData);
			parser.Parse();
			Assert.That(File.Exists(parser.OutputFileForTest));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		[Test]
		public void RunProcessWithWarnings()
		{
			var odsData = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodesInvalid.ods");
			parser = new UKOfficeCodesParserForTest(errorCollector, odsData);
			parser.Parse();
			Assert.That(File.Exists(parser.OutputFileForTest));

			var expectedErrorMessage = new StringBuilder()
				.AppendLine("Unable to import the following UK Customs Office records:")
				.AppendLine("- Code: GB000014 | Description:  | Usual Name:  | City:  | Region: NORTHERN IRELAND").ToString();
			var errorMessage = errorCollector.ToString();
			Assert.That(errorMessage, Is.Not.Empty);
			Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));
		}

		[Test]
		public void RunProcessFail()
		{
			var odsData = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodesBad.ods");
			parser = new UKOfficeCodesParserForTest(errorCollector, odsData);
			parser.Parse();
			Assert.That(!File.Exists(parser.OutputFileForTest));

			var expectedErrorMessage = new StringBuilder().AppendLine("Unable to import any UK Customs Office records. No valid data found!").ToString();
			var errorMessage = errorCollector.ToString();
			Assert.That(errorMessage, Is.Not.Empty);
			Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));
		}

		[SetUp]
		public void Setup()
		{
			errorCollector = new StringBuilder();
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(parser.OutputFileForTest))
			{
				File.Delete(parser.OutputFileForTest);
			}
		}

		StringBuilder errorCollector;
		UKOfficeCodesParserForTest parser;
	}
}
