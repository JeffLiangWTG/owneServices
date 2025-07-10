using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class TariffXmlProducingStratagyTest
	{
		[Test]
		public void ProduceXml_LogsLastProcessedDate()
		{
			Assert.Multiple(() =>
			{
				AssertLogs(new DateTime(2024, 6, 13));
				AssertLogs(null);
			});

			void AssertLogs(DateTime? lastTariffDate)
			{
				lastProcessDateStorageMock.Setup(x => x.Load<DateTime?>()).Returns(lastTariffDate);
				strategy.ProduceXml();
				loggerMock.Verify(x => x.Log(LogType.Info, "Last processed date", lastTariffDate), Times.Once);
			}
		}

		[Test]
		public void ProduceXml_LastTariffDateIsNullOrOlder()
		{
			Assert.Multiple(() =>
			{
				AssertLogs(null, new DateTime(2023, 2, 1));
				AssertLogs(new DateTime(2023, 2, 1), new DateTime(2023, 2, 1));
			});

			void AssertLogs(DateTime? lastTariffDate, DateTime? currentTariffDate)
			{
				const string jsonFilesCreationFolder = "TestJsonFolder";
				const string pdfDownloadFolder = "PdfFolder";
				var refData = new Dictionary<string, List<RefCusTariff>>();
				lastProcessDateStorageMock.Setup(s => s.Load<DateTime?>()).Returns(lastTariffDate);
				downloaderMock.Setup(d => d.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
				downloaderMock.Setup(d => d.DownloadPdfFiles(currentTariffDate.Value)).Returns(pdfDownloadFolder);
				var tempOutFolder = TestHelper.CreateTempFolder();
				var tempOutFile = Path.Combine(tempOutFolder, "OutFile.xml");

				parserMock.Setup(p => p.ParseFilesAsJson(pdfDownloadFolder, currentTariffDate)).Returns(jsonFilesCreationFolder);
				locationProviderMock.Setup(x => x.GetOutputTariffXmlFilePath(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(tempOutFile);
				builderMock.Setup(x => x.GetRefData(jsonFilesCreationFolder)).Returns(refData);

				strategy.ProduceXml();

				loggerMock.Verify(l => l.Log(LogType.Info, "Downloaded data to folder", pdfDownloadFolder), Times.Once);
				loggerMock.Verify(l => l.Log(LogType.Info, "Parsed json data to folder", jsonFilesCreationFolder), Times.Once);
				lastProcessDateStorageMock.Verify(l => l.Save(currentTariffDate), Times.Once);
				Directory.Delete(tempOutFolder, true);
			}
		}

		[Test]
		public void ProduceXml_LastTariffDateIsSameAsCurrentTariffDate_NoNewData()
		{
			DateTime? lastTariffDate = new DateTime(2023, 2, 1);
			DateTime? currentTariffDate = new DateTime(2023, 2, 1);
			lastProcessDateStorageMock.Setup(s => s.Load<DateTime?>()).Returns(lastTariffDate);
			downloaderMock.Setup(d => d.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);

			strategy.ProduceXml();

			loggerMock.Verify(l => l.Log(LogType.ReviewRequired, "No new data found and last Tariff date is on", lastTariffDate), Times.Once);
			downloaderMock.Verify(x => x.DownloadPdfFiles(It.IsAny<DateTime>()), Times.Never);
		}

		[Test]
		public void ProduceXml_DownloadFail_NoDataDownloaded()
		{
			var lastTariffDate = DateTime.Now.AddDays(-1);
			var currentTariffDate = DateTime.Now;
			lastProcessDateStorageMock.Setup(x => x.Load<DateTime?>()).Returns(lastTariffDate);
			downloaderMock.Setup(x => x.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
			downloaderMock.Setup(x => x.DownloadPdfFiles(currentTariffDate)).Returns((string)null);

			strategy.ProduceXml();

			loggerMock.Verify(x => x.Log(LogType.ReviewRequired, "No data downloaded for date", currentTariffDate), Times.Once);
		}

		[Test]
		public void ProduceXml_DownloadPdfFilesDisabled_UsesFolderFromLocationProvider()
		{
			strategy = new TariffXmlProducingStratagy(
							"TestDataSource",
							downloaderMock.Object,
							parserMock.Object,
							builderMock.Object,
							locationProviderMock.Object,
							dateTimeProviderMock.Object,
							lastProcessDateStorageMock.Object,
							false,
							true,
							true,
							loggerMock.Object);

			DateTime? lastTariffDate = new DateTime(2023, 1, 1);
			lastProcessDateStorageMock.Setup(s => s.Load<DateTime?>()).Returns(lastTariffDate);
			locationProviderMock.Setup(lp => lp.GetPdfDownloadFolder(lastTariffDate)).Returns("PdfFolder");

			strategy.ProduceXml();

			loggerMock.Verify(l => l.Log(LogType.Info, "Pdf download skipped as per cofiguration", It.IsAny<object>()), Times.Once);
		}


		[Test]
		public void ProduceXml_ProcessPdfFails_NoDataParsed()
		{
			var lastTariffDate = DateTime.Now.AddDays(-1);
			var currentTariffDate = DateTime.Now;
			var pdfDownloadsFolder = "TestPdfFolder";

			lastProcessDateStorageMock.Setup(x => x.Load<DateTime?>()).Returns(lastTariffDate);
			downloaderMock.Setup(x => x.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
			downloaderMock.Setup(x => x.DownloadPdfFiles(currentTariffDate)).Returns(pdfDownloadsFolder);
			parserMock.Setup(x => x.ParseFilesAsJson(pdfDownloadsFolder, currentTariffDate)).Returns((string)null);

			strategy.ProduceXml();

			loggerMock.Verify(x => x.Log(LogType.ReviewRequired, "No data parsed for date", currentTariffDate), Times.Once);
		}

		[Test]
		public void ProduceXml_GeneratesRefDataXml_Success()
		{
			var lastTariffDate = DateTime.Now.AddDays(-1);
			var currentTariffDate = DateTime.Now;
			var pdfDownloadsFolder = "TestPdfFolder";
			var jsonFilesCreationFolder = "TestJsonFolder";
			var refData = new Dictionary<string, List<RefCusTariff>>();
			var tempOutFolder = TestHelper.CreateTempFolder();
			var tempOutFile = Path.Combine(tempOutFolder, "OutFile.xml");

			lastProcessDateStorageMock.Setup(x => x.Load<DateTime?>()).Returns(lastTariffDate);
			downloaderMock.Setup(x => x.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
			downloaderMock.Setup(x => x.DownloadPdfFiles(currentTariffDate)).Returns(pdfDownloadsFolder);
			parserMock.Setup(x => x.ParseFilesAsJson(pdfDownloadsFolder, currentTariffDate)).Returns(jsonFilesCreationFolder);
			builderMock.Setup(x => x.GetRefData(jsonFilesCreationFolder)).Returns(refData);
			dateTimeProviderMock.Setup(x => x.GetIndiaToday()).Returns(DateTime.Now);
			locationProviderMock.Setup(x => x.GetOutputTariffXmlFilePath(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(tempOutFile);

			strategy.ProduceXml();

			builderMock.Verify(x => x.GetRefData(jsonFilesCreationFolder), Times.Once);
			loggerMock.Verify(x => x.Log(LogType.Info, "Parsed json data to folder", jsonFilesCreationFolder), Times.Once);
			Directory.Delete(tempOutFolder, true);
		}

		[Test]
		public void ProduceXml_GeneratesMultipleRefDataXml_Success()
		{
			var lastTariffDate = new DateTime(2025, 01, 01);
			var currentTariffDate = new DateTime(2025, 01, 02);
			var pdfDownloadsFolder = "TestPdfFolder";
			var jsonFilesCreationFolder = "TestJsonFolder";
			var refData = new Dictionary<string, List<RefCusTariff>>();
			var tariffs01 = new List<RefCusTariff>
			{
				new RefCusTariff { ZZ1_TariffCode = "01010101", ZZ1_Description = "01010101 Desc"},
				new RefCusTariff { ZZ1_TariffCode = "01010102", ZZ1_Description = "01010101 Desc"},
				new RefCusTariff { ZZ1_TariffCode = "01010103", ZZ1_Description = "01010101 Desc"}
			};
			var tariffs09 = new List<RefCusTariff>
			{
				new RefCusTariff { ZZ1_TariffCode = "09010101", ZZ1_Description = "09010101 Desc"},
				new RefCusTariff { ZZ1_TariffCode = "09010102", ZZ1_Description = "09010101 Desc"}
			};
			var tariffs97 = new List<RefCusTariff>
			{
				new RefCusTariff { ZZ1_TariffCode = "97010101", ZZ1_Description = "97010101 Desc"}
			};
			refData.Add("01", tariffs01);
			refData.Add("09", tariffs09);
			refData.Add("97", tariffs97);

			var tempOutFolder = TestHelper.CreateTempFolder();
			try
			{
				var tempOutFile01 = Path.Combine(tempOutFolder, "OutFile01.xml");
				var tempOutFile09 = Path.Combine(tempOutFolder, "OutFile09.xml");
				var tempOutFile97 = Path.Combine(tempOutFolder, "OutFile97.xml");
				lastProcessDateStorageMock.Setup(x => x.Load<DateTime?>()).Returns(lastTariffDate);
				downloaderMock.Setup(x => x.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
				downloaderMock.Setup(x => x.DownloadPdfFiles(currentTariffDate)).Returns(pdfDownloadsFolder);
				parserMock.Setup(x => x.ParseFilesAsJson(pdfDownloadsFolder, currentTariffDate)).Returns(jsonFilesCreationFolder);
				builderMock.Setup(x => x.GetRefData(jsonFilesCreationFolder)).Returns(refData);
				dateTimeProviderMock.Setup(x => x.GetIndiaToday()).Returns(currentTariffDate);
				locationProviderMock.Setup(x => x.GetOutputTariffXmlFilePath("01", It.IsAny<DateTime>())).Returns(tempOutFile01);
				locationProviderMock.Setup(x => x.GetOutputTariffXmlFilePath("09", It.IsAny<DateTime>())).Returns(tempOutFile09);
				locationProviderMock.Setup(x => x.GetOutputTariffXmlFilePath("97", It.IsAny<DateTime>())).Returns(tempOutFile97);

				strategy.ProduceXml();
				var actualOutput01 = File.ReadAllText(tempOutFile01);
				var expectedOutput01 = TestHelper.ReadContentString("Tariff\\INTestFiles\\Output\\OutFile01.xml");
				Assert.That(actualOutput01, Is.EqualTo(expectedOutput01).NoClip, "Tariff XML 01");
				var actualOutput09 = File.ReadAllText(tempOutFile09);
				var expectedOutput09 = TestHelper.ReadContentString("Tariff\\INTestFiles\\Output\\OutFile09.xml");
				Assert.That(actualOutput09, Is.EqualTo(expectedOutput09).NoClip, "Tariff XML 09");
				var actualOutput97 = File.ReadAllText(tempOutFile97);
				var expectedOutput97 = TestHelper.ReadContentString("Tariff\\INTestFiles\\Output\\OutFile97.xml");
				Assert.That(actualOutput97, Is.EqualTo(expectedOutput97).NoClip, "Tariff XML 97");
			}
			finally
			{
				Directory.Delete(tempOutFolder, true);
			}
		}

		[Test]
		public void ProduceXml_ParsePdfFilesDisabled_UsesFolderFromLocationProvider()
		{
			strategy = new TariffXmlProducingStratagy(
							"TestDataSource",
							downloaderMock.Object,
							parserMock.Object,
							builderMock.Object,
							locationProviderMock.Object,
							dateTimeProviderMock.Object,
							lastProcessDateStorageMock.Object,
							true,
							false,
							true,
							loggerMock.Object);

			DateTime? lastTariffDate = new DateTime(2023, 1, 1);
			DateTime? currentTariffDate = new DateTime(2023, 2, 1);
			var refData = new Dictionary<string, List<RefCusTariff>>();
			var tempOutFolder = TestHelper.CreateTempFolder();
			var tempOutFile = Path.Combine(tempOutFolder, "OutFile.xml");

			lastProcessDateStorageMock.Setup(s => s.Load<DateTime?>()).Returns(lastTariffDate);
			downloaderMock.Setup(d => d.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
			downloaderMock.Setup(d => d.DownloadPdfFiles(currentTariffDate.Value)).Returns("PdfFolder");
			builderMock.Setup(x => x.GetRefData(It.IsAny<string>())).Returns(refData);
			locationProviderMock.Setup(x => x.GetOutputTariffXmlFilePath(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(tempOutFile);

			strategy.ProduceXml();
			loggerMock.Verify(x => x.Log(LogType.Info, "PDF parsing skipped as per configuration", It.IsAny<object>()), Times.Once);
			Directory.Delete(tempOutFolder, true);
		}

		[Test]
		public void ProduceXml_GenerateRefDataXmlDisabled_SkipsXmlGeneration()
		{
			strategy = new TariffXmlProducingStratagy(
							"TestDataSource",
							downloaderMock.Object,
							parserMock.Object,
							builderMock.Object,
							locationProviderMock.Object,
							dateTimeProviderMock.Object,
							lastProcessDateStorageMock.Object,
							true,
							true,
							false,
							loggerMock.Object);

			DateTime? lastTariffDate = new DateTime(2023, 1, 1);
			DateTime? currentTariffDate = new DateTime(2023, 2, 1);
			lastProcessDateStorageMock.Setup(s => s.Load<DateTime?>()).Returns(lastTariffDate);
			downloaderMock.Setup(d => d.GetLatestTariffDate(lastTariffDate)).Returns(currentTariffDate);
			downloaderMock.Setup(d => d.DownloadPdfFiles(currentTariffDate.Value)).Returns("PdfFolder");

			parserMock.Setup(p => p.ParseFilesAsJson("PdfFolder", currentTariffDate)).Returns("JsonFolder");

			strategy.ProduceXml();

			builderMock.Verify(b => b.GetRefData(It.IsAny<string>()), Times.Never);
		}

		[SetUp]
		public void SetUp()
		{
			downloaderMock = new Mock<ITariffPdfDownloader>();
			parserMock = new Mock<ITariffPdfParser>();
			builderMock = new Mock<IRefTariffDataBuilder>();
			locationProviderMock = new Mock<IFolderLocationProvider>();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			lastProcessDateStorageMock = new Mock<ILocalFileStorage>();
			loggerMock = new Mock<ILogger>();
			strategy = new TariffXmlProducingStratagy(
								"TestDataSource",
								downloaderMock.Object,
								parserMock.Object,
								builderMock.Object,
								locationProviderMock.Object,
								dateTimeProviderMock.Object,
								lastProcessDateStorageMock.Object,
								downloadPdfFiles: true,
								processPdfFiles: true,
								generateRefDataXml: true,
								loggerMock.Object);
		}

		Mock<ITariffPdfDownloader> downloaderMock;
		Mock<ITariffPdfParser> parserMock;
		Mock<IRefTariffDataBuilder> builderMock;
		Mock<IFolderLocationProvider> locationProviderMock;
		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<ILocalFileStorage> lastProcessDateStorageMock;
		Mock<ILogger> loggerMock;
		TariffXmlProducingStratagy strategy;
	}
}
