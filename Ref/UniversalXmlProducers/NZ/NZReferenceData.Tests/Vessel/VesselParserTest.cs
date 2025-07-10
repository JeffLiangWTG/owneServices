using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	[TestFixture]
	public class VesselParserTest
	{
		[Test]
		public void TestVesselList()
		{
			var localFilePath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"Vessel\TestFiles\Input\VesselList.txt");
			var expectedImportXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NZReferenceData.Tests.Vessel.TestFiles.Output.RefCusCodeListZZ_NZ_Vessel.xml");

			var outputFolderPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"TestFiles");
			Directory.CreateDirectory(outputFolderPath);

			var outputFileFullName = Path.Combine(outputFolderPath, "RefCusCodeListZZ_NZ_Vessel.xml");
			File.Delete(outputFileFullName);

			VesselParser.ExportToXMLFile(localFilePath, outputFolderPath, new DateTime(2020, 05, 01));

			var actualOutputXml = File.ReadAllText(outputFileFullName);
			Assert.That(actualOutputXml, Is.EqualTo(expectedImportXML));
		}

		public void TestTempFileDeletedAfterRun()
		{
			var localFilePath = Path.GetTempFileName();

			new VesselListProgram().Run();
			Assert.False(File.Exists(localFilePath), "Temporary file should be deleted after Vessel List Program run.");
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
