using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class RefTariffDataBuilderTest
	{
		[Test]
		public void TestGetRefData()
		{
			Assert.Multiple(() =>
			{
				var result = Builder.GetRefData("XYZ");
				Assert.AreEqual(0, result.Count, "Invalid location");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Folder XYZ does not exist", It.IsAny<object>()), Times.Once);

				var tempFolder = TestHelper.CreateTempFolder();
				result = Builder.GetRefData(tempFolder);
				Assert.AreEqual(0, result.Count, "Empty folder");

				var invalidJson = Path.Combine(tempFolder, "invalid.json");
				File.WriteAllText(invalidJson, "invalid");
				result = Builder.GetRefData(tempFolder);
				Assert.AreEqual(0, result.Count, "Invalid Json");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, $"Skipping the file {invalidJson}, because ", It.IsAny<object>()), Times.Once);

				var nonDeclarableJson = Path.Combine(tempFolder, "NonDeclarableTariff.json");
				File.WriteAllText(nonDeclarableJson, TariffDataNonDeclarableJson);
				result = Builder.GetRefData(tempFolder);
				Assert.AreEqual(0, result.Count, "Valid Json, without declarable codes");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, $"No data found in JSON file {nonDeclarableJson}", It.IsAny<object>()), Times.Once);

				var declarableJson = Path.Combine(tempFolder, "DeclarableTariff.json");
				File.WriteAllText(declarableJson, TariffDataDeclarableJson);
				result = Builder.GetRefData(tempFolder);
				var tariffs = result.SelectMany(x => x.Value).ToArray();
				Assert.AreEqual(1, tariffs.Length, "Valid Json, with declarable codes");
				Assert.AreEqual("NOS", tariffs[0].RefCusTariffUOMs[0].ZZ8_UOM, "TariffItem");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, $"Missing chapters in Tariff data", It.IsAny<object>()), Times.Once);

				var unknownUnitJson = Path.Combine(tempFolder, "UnknownUnitTariff.json");
				File.WriteAllText(unknownUnitJson, TariffDataUnknownUnitJson);
				result = Builder.GetRefData(tempFolder);
				tariffs = result.SelectMany(x => x.Value).ToArray();
				Assert.AreEqual(2, tariffs.Length, "Valid Json, with unknown unit");
				Assert.IsNull(tariffs.FirstOrDefault(x => x.ZZ1_TariffCode == "01012111").RefCusTariffUOMs, "UOM");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Unknown Unit", It.IsAny<object>()), Times.Once);

				var tariffDate = new DateTime(2024, 6, 13);
				var xmlWriterConfig = XMLWriterHelper.GetRefTariffWriterConfiguration(tariffDate);

				var outputXmlPath = Path.Combine(tempFolder, "output.xml");
				XMLWriterHelper.ExportToXMLFile(xmlWriterConfig, tariffs, "Tariff Test", tariffDate, UpdateType.Full, outputXmlPath);
				var actualOutput = File.ReadAllText(outputXmlPath);
				var expectedOutput = TestHelper.ReadContentString("Tariff\\INTestFiles\\Output\\TariffOutput.xml");
				Assert.That(actualOutput, Is.EqualTo(expectedOutput).NoClip, "Tariff XML");

				Directory.Delete(tempFolder, true);
			});
		}

		RefTariffDataBuilder Builder => builder ?? (builder = new RefTariffDataBuilder(LoggerMock.Object));
		RefTariffDataBuilder builder;

		Mock<ILogger> LoggerMock => loggerMock ?? (loggerMock = new Mock<ILogger>());
		Mock<ILogger> loggerMock;

		#region Json Data

		const string TariffDataNonDeclarableJson = @"[
  {
    ""tariffItem"": ""0101"",
    ""hyphens"": """",
    ""description"": ""LIVE HORSES, ASSES, MULES AND HINNIES"",
    ""unit"": """",
    ""standardRate"": """",
    ""preferentialRate"": """"
  },
  {
    ""tariffItem"": """",
    ""hyphens"": ""-"",
    ""description"": ""Horses:"",
    ""unit"": """",
    ""standardRate"": """",
    ""preferentialRate"": """"
  }
]";

		const string TariffDataDeclarableJson = @"[
  {
    ""tariffItem"": ""0101"",
    ""hyphens"": """",
    ""description"": ""LIVE HORSES, ASSES, MULES AND HINNIES"",
    ""unit"": """",
    ""standardRate"": """",
    ""preferentialRate"": """"
  },
  {
    ""tariffItem"": """",
    ""hyphens"": ""-"",
    ""description"": ""Horses:"",
    ""unit"": """",
    ""standardRate"": """",
    ""preferentialRate"": """"
  },
  {
    ""tariffItem"": ""0101 21 00"",
    ""hyphens"": ""--"",
    ""description"": ""Pure-bred breeding animals:"",
    ""unit"": ""u"",
    ""standardRate"": ""*Free"",
    ""preferentialRate"": ""-""
  }
]";

		const string TariffDataUnknownUnitJson = @"[
  {
    ""tariffItem"": ""0101"",
    ""hyphens"": """",
    ""description"": ""LIVE HORSES, ASSES, MULES AND HINNIES"",
    ""unit"": """",
    ""standardRate"": """",
    ""preferentialRate"": """"
  },
  {
    ""tariffItem"": """",
    ""hyphens"": ""-"",
    ""description"": ""Horses:"",
    ""unit"": """",
    ""standardRate"": """",
    ""preferentialRate"": """"
  },
  {
    ""tariffItem"": ""0101 21 11"",
    ""hyphens"": ""--"",
    ""description"": ""Mix-bred breeding animals:"",
    ""unit"": ""A"",
    ""standardRate"": ""*Free"",
    ""preferentialRate"": ""-""
  }
]";

		#endregion
	}
}
