using System;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class TariffPdfParserTest
	{
		[Test]
		public void TestParseFilesAsJson()
		{
			Assert.Multiple(() =>
			{
				Assert.IsNull(Parser.ParseFilesAsJson(null, null), "Invalid location and date");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Invalid folder path", It.IsAny<string>()), Times.Once);

				Assert.IsNull(Parser.ParseFilesAsJson("XYZ", null), "Invalid date");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Invalid folder path", It.IsAny<string>()), Times.Exactly(2));

				var tempJsonFolder = TestHelper.CreateTempFolder();
				MockLocationProvider.Setup(x => x.GetJsonCreationFolder(It.IsAny<DateTime>())).Returns(tempJsonFolder);
				Assert.IsNull(Parser.ParseFilesAsJson(null, new DateTime(2024, 6, 30)), "Invalid location");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Invalid folder path", It.IsAny<string>()), Times.Exactly(3));

				Assert.IsNull(Parser.ParseFilesAsJson("XYZ", new DateTime(2024, 6, 30)), "Invalid location");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Folder XYZ doesnot exist", It.IsAny<string>()), Times.Once);

				var tempPdfFolder = TestHelper.CreateTempFolder();
				Assert.AreEqual(tempJsonFolder, Parser.ParseFilesAsJson(tempPdfFolder, new DateTime(2024, 6, 30)), "Valid empty location");
				TestHelper.AssertFileCount(tempJsonFolder, 0, "No files to process");

				AddPdfFilesToProcess(tempPdfFolder);
				Parser.ParseFilesAsJson(tempPdfFolder, new DateTime(2024, 6, 30));
				TestHelper.AssertFileCount(tempJsonFolder, testFileList.Length, "files processed");
				AssertFilesContent(tempJsonFolder);

				Directory.Delete(tempPdfFolder, true);
				Directory.Delete(tempJsonFolder, true);
			});
		}

		void AddPdfFilesToProcess(string folderPath)
		{
			foreach (var fileName in testFileList)
			{
				File.WriteAllBytes(Path.Combine(folderPath, $"{fileName}.pdf"), TestHelper.ReadContentBytes($"Tariff\\INTestFiles\\Input\\{fileName}.pdf"));
			}
		}

		void AssertFilesContent(string tempJsonFolder)
		{
			foreach (var fileName in testFileList)
			{
				var expectedFile = Path.Combine(tempJsonFolder, $"{fileName}.json");
				Assert.IsTrue(File.Exists(expectedFile), $"{fileName} doesnot exist");
				Assert.That(File.ReadAllText(expectedFile), Is.EqualTo(TestHelper.ReadContentString($"Tariff\\INTestFiles\\Output\\{fileName}.json")).NoClip, fileName);
			}
		}

		readonly string[] testFileList = ["chap2", "chap3", "chap4", "chap7", "chap9", "chap10",
													"chap11", "chap14", "chap15", "chap17",
													"chap21", "chap23", "chap25", "chap27", "chap29",
													"chap32", "chap37", "chap38", "chap39", "chap40",
													"chap41", "chap44", "chap45", "chap48",
													"chap52", "chap54", "chap55", "chap57", "chap58",
													"chap62", "chap66",
													"chap70", "chap72", "chap73", "chap74",
													"chap84", "chap85",
													"chap90", "chap96", "chap97", "chap98"];

		TariffPdfParser Parser => parser ??= new TariffPdfParser(MockLocationProvider.Object, LoggerMock.Object);
		TariffPdfParser parser;

		Mock<IFolderLocationProvider> MockLocationProvider => mockLocationProvider ??= new Mock<IFolderLocationProvider>();
		Mock<IFolderLocationProvider> mockLocationProvider;

		Mock<ILogger> LoggerMock => loggerMock ??= new Mock<ILogger>();
		Mock<ILogger> loggerMock;
	}
}
