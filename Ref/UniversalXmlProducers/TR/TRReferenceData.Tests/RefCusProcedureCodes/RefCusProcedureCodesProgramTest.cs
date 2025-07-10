using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.TRReferenceData.Business.RefCusProcedureCodesParser;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	public class RefCusProcedureCodesProgramTest
	{
		[Test]
		public void Run()
		{
			TRReferenceData.CmdLine.RefCusProcedureCodesProgram.Run( DataFileName, OutputileName);
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.RefCusProcedureCodes.TestFiles.Output.RefCusProcedureCodes_TR.xml");
			Assert.AreEqual(expectedXML, File.ReadAllText(OutputileName));
		}

		[Test]
		public void ExceptionTest()
		{
			var parser = new RefCusProcedureCodesParserExceptionTest( DataFileName);
			parser.GenerateUXML("");
			Assert.That(parser.ErrorMessage, Does.Contain("Failed to generate UXML"));
			Assert.That(parser.ErrorMessage, Does.Contain("Invalid data."));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.RefCusProcedureCodes.TestFiles.Input.TR - RefCusProcedureCodes.xlsx");
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

		string DataFileName => Path.Combine(tempFolder, "TR - RefCusProcedureCodes.xlsx");

		string OutputileName => Path.Combine(tempFolder, "RefCusProcedureCodes_TR.xml");

		public class RefCusProcedureCodesParserExceptionTest : RefCusProcedureCodesParser
		{
			public RefCusProcedureCodesParserExceptionTest( string dataFileName) : base(dataFileName)
			{
			}

			protected override RefDataRepoModelEntityType[] GetEntities()
			{
				throw new InvalidDataException("Invalid data.");
			}
		}
	}
}
