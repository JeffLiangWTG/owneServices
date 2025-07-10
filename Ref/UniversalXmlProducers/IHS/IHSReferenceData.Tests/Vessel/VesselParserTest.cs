using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IHSReferenceData.Tests.Vessel
{
	[TestFixture]
	public class VesselParserTest
	{
		[Test]
		public void ExportToXMLFile()
		{
			var sampleVesselsCsv = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IHSReferenceData.Tests.Vessel.TestFiles.Input.ShipDataSample.CSV");
			var flagCodesCsv = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IHSReferenceData.Tests.Vessel.TestFiles.Input.tblFlagCodes.CSV");
			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.IHSReferenceData.Tests.Vessel.TestFiles.Output.RefCusCodeListZZ_IHS_Vessel.xml");
			var outputFolderPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"TestFiles");
			Directory.CreateDirectory(outputFolderPath);
			var outputFileFullName = Path.Combine(outputFolderPath, "RefCusCodeListZZ_IHS_Vessel.xml");

			VesselParser.ExportToXMLFile(sampleVesselsCsv, flagCodesCsv, outputFolderPath, new DateTime(2020, 11, 20));

			Assert.That(expectedImportXML, Is.EqualTo(File.ReadAllText(outputFileFullName)));

			File.Delete(outputFileFullName);
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
