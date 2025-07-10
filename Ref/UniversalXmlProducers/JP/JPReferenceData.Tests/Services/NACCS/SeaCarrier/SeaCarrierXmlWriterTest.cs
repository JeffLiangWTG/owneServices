using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class SeaCarrierXmlWriterTest
	{
		readonly string inputFilePath = @"TestFiles\funaka_n.csv";
		readonly string vesselFilePath = @"TestFiles\senpaku_n.csv";
		readonly string expectedCarrierOutputFilePath = @"TestFiles\Expected_JP_SeaCarrier.xml";
		readonly string actualCarrierOutputFilePath = "JPSeaCarrier.xml";
		readonly string expectedVesselOutputFilePath = @"TestFiles\Expected_JP_SeaVessel.xml";
		readonly string actualVesselOutputFilePath = "JPSeaVessel.xml";

		[Test]
		public void TestParseAndSaveXml()
		{
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedCarrierFilePath = Path.Combine(dirPath, expectedCarrierOutputFilePath);
			var expectedVesselFilePath = Path.Combine(dirPath, expectedVesselOutputFilePath);
			var actualCarrierFilePath = Path.Combine(AppConfig.Shared.OutputDirectory, actualCarrierOutputFilePath);
			var actualVesselFilePath = Path.Combine(AppConfig.Shared.OutputDirectory, actualVesselOutputFilePath);

			var expectedCarrierXmlAsString = File.ReadAllText(expectedCarrierFilePath);
			var expectedVesselXmlAsString = File.ReadAllText(expectedVesselFilePath);

			SeaCarrierXmlWriter.ParseAndSaveXml(inputFilePath, vesselFilePath, new DateTime(24, 12, 17));

			var actualCarrierXmlAsString = File.ReadAllText(actualCarrierFilePath);
			Assert.That(actualCarrierXmlAsString, Is.EqualTo(expectedCarrierXmlAsString));
			File.Delete(actualCarrierFilePath);

			var actualVesselXmlAsString = File.ReadAllText(actualVesselFilePath);
			Assert.That(actualVesselXmlAsString, Is.EqualTo(expectedVesselXmlAsString));
			File.Delete(actualCarrierFilePath);
		}
	}
}
