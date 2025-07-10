using System;
using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	public class TradeGroupProgramTest
	{
		[Test]
		public void Run()
		{
			TRReferenceData.CmdLine.TradeGroupProgram.Run(DateTimeProvider, DataFileName, OutputileName);
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.TradeGroups.TestFiles.Output.RefCusTradeGroupZZ_TR.xml");
			Assert.AreEqual(expectedXML, File.ReadAllText(OutputileName));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.TradeGroups.TestFiles.Input.TradeGroupData.xlsx");
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

		string DataFileName => Path.Combine(tempFolder, "TradeGroupData.xlsx");

		string OutputileName => Path.Combine(tempFolder, "RefCusTradeGroupZZ_TR.xml");

		DateTime Now => new DateTime(2023, 07, 18, 09, 38, 23);

		IDateTimeProvider DateTimeProvider => TestHelper.MockDateTimeProvider(Now);
	}
}
