using System;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.TRReferenceData.Business.DutyCodesParser;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	public class DutyCodesProgramTest
	{
		[Test]
		public void Run()
		{
			TRReferenceData.CmdLine.DutyCodesProgram.Run(DateTimeProvider, DataFileName, OutputileName);
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.DutyCodes.TestFiles.Output.DutyCodes_TR.xml");
			Assert.AreEqual(expectedXML, File.ReadAllText(OutputileName));
		}

		[Test]
		public void ExceptionTest()
		{
			var parser = new DutyCodesParserExceptionTest(DateTimeProvider, DataFileName);
			parser.GenerateUXML("");
			Assert.That(parser.ErrorMessage, Does.Contain("Failed to generate UXML"));
			Assert.That(parser.ErrorMessage, Does.Contain("Invalid data."));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.DutyCodes.TestFiles.Input.DutyCodes.xlsx");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;

		string DataFileName => Path.Combine(tempFolder, "DutyCodes.xlsx");

		string OutputileName => Path.Combine(tempFolder, "DutyCodes_TR.xml");

		DateTime Now => new DateTime(2025, 01, 01, 00, 00, 00);

		IDateTimeProvider DateTimeProvider => TestHelper.MockDateTimeProvider(Now);

		public class DutyCodesParserExceptionTest : DutyCodesParser
		{
			public DutyCodesParserExceptionTest(IDateTimeProvider dateTimeProvider, string dataFileName) : base(dateTimeProvider, dataFileName)
			{
			}

			protected override RefDataRepoModelEntityType[] GetEntities()
			{
				throw new InvalidDataException("Invalid data.");
			}
		}
	}
}
