using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	public class DailyTariffFactoryFixture
	{
		DailyTariffFactory dailyTariffFactory;

		[Test]
		public void ExtractPublishTime()
		{
			var htmlContent = File.ReadAllText(DailyTariffTestFiles.DailyTaricWebPageHtmlFile);
			var webDriverHelperMock = new Mock<IWebDriverHelper>();
			webDriverHelperMock.Setup(x => x.GetWebPage(It.IsAny<string>(), 0)).Returns(htmlContent);
			var result = dailyTariffFactory.ExtractDailyZipPublishTimeFromTaricWebSite(webDriverHelperMock.Object);

			Assert.That(result, Is.EqualTo(new DateTime(2022, 04, 01)));
		}

		[Test]
		public void ExtractPublishTimeException()
		{
			var webDriverHelperMock = new Mock<IWebDriverHelper>();
			webDriverHelperMock.Setup(x => x.GetWebPage(It.IsAny<string>(), 0)).Returns("invalid content");
			var exception = Assert.Throws<InvalidOperationException>(() => dailyTariffFactory.ExtractDailyZipPublishTimeFromTaricWebSite(webDriverHelperMock.Object));
			Assert.IsNotNull(exception);
			Assert.That(exception.Message, Is.EqualTo("Could not read the Daily publish time from the web site"));
		}

		[Test]
		[SetCulture("en-US")]
		public void ExtractPublishTimeCorrectFormatAnyCulture()
		{
			var htmlContent = File.ReadAllText(DailyTariffTestFiles.DailyTaricWebPageHtmlFile);
			var webDriverHelperMock = new Mock<IWebDriverHelper>();
			webDriverHelperMock.Setup(x => x.GetWebPage(It.IsAny<string>(), 0)).Returns(htmlContent);
			var result = dailyTariffFactory.ExtractDailyZipPublishTimeFromTaricWebSite(webDriverHelperMock.Object);

			Assert.That(result, Is.EqualTo(new DateTime(2022, 04, 01)));
		}

		[Test]
		public void GetPreDownloadedFiles()
		{
			var fileNames = new[] { "MonthlyDutiesImport.xlsx", "MonthlyMeasureConditions.xlsx", "MonthlyMeasureExclusions.xlsx" };
			var creationTime = DateTime.Now.AddMonths(-2);
			var lastWriteTime = DateTime.Now.AddMonths(-1);
			DailyTariffTestFiles.SetCreationTime(creationTime, fileNames);
			DailyTariffTestFiles.SetLastWriteTime(lastWriteTime, fileNames);
			var dailyTariffFactoryWithDownloadFolder = new DailyTariffFactory(new[] { new RefCusTariff() }, DailyTariffTestFiles.BaseDirectory);
			var preDownloadedFiles = dailyTariffFactoryWithDownloadFolder.GetPreDownloadedFiles();
			Assert.NotNull(preDownloadedFiles);
			foreach (var fileName in fileNames)
			{
				var webFileInfo = preDownloadedFiles.First(x => x.FileName == fileName);
				Assert.NotNull(webFileInfo);
				Assert.AreEqual(lastWriteTime, webFileInfo.LastModificationTime);
			}

			var emptyPreDownloadedFiles = dailyTariffFactory.GetPreDownloadedFiles();
			Assert.False(emptyPreDownloadedFiles.Any());
		}

		[TestCase(true, 0)]
		[TestCase(true, 1)]
		[TestCase(true, 2)]
		[TestCase(false, 2)]
		public void GetOrDownloadMonthlyFiles(bool downloadSucceed, int month)
		{
			ApplicationConfig.SetDownloadsFolder(DailyTariffTestFiles.BaseDirectory);
			var fileName1 = @"MonthlyDutiesImport.xlsx";
			var fileName2 = @"MonthlyMeasureConditions.xlsx";
			var fileName3 = @"MonthlyMeasureExclusions.xlsx";
			var filesToLocate = new[] { fileName1, fileName2, fileName3 };
			var lastWriteTime = DateTime.Now.AddMonths(-month);
			var fileInfo1 = new WebFileInfo(fileName1, DailyTariffTestFiles.MonthlyDutiesImportPath, lastWriteTime);
			var fileInfo2 = new WebFileInfo(fileName2, DailyTariffTestFiles.MonthlyMeasuresConditionPath, lastWriteTime);
			var fileInfo3 = new WebFileInfo(fileName3, DailyTariffTestFiles.MonthlyMeasuresExclusionPath, lastWriteTime);

			var fileInfos = new WebFileInfo[] { fileInfo1, fileInfo2, fileInfo3 };
			var monthlyProducerMock = new Mock<IProducer>();
			monthlyProducerMock.Setup(x => x.FilesToLocate).Returns(filesToLocate);
			monthlyProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			monthlyProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(downloadSucceed);

			DailyTariffTestFiles.SetLastWriteTime(lastWriteTime, filesToLocate);
			if (!downloadSucceed)
			{
				var exception = Assert.Throws<InvalidOperationException>(() => dailyTariffFactory.GetOrDownloadMonthlyFiles(monthlyProducerMock.Object));
				Assert.AreEqual("Failure downloading files.", exception.Message);
			}
			else
			{
				var results = dailyTariffFactory.GetOrDownloadMonthlyFiles(monthlyProducerMock.Object);
				if (month > 1)
				{
					monthlyProducerMock.Verify(x => x.LocateWebFiles(), Times.Once);
					monthlyProducerMock.Verify(x => x.DownloadFiles(fileInfos), Times.Once);
				}
				else
				{
					monthlyProducerMock.Verify(x => x.LocateWebFiles(), Times.Never);
					monthlyProducerMock.Verify(x => x.DownloadFiles(fileInfos), Times.Never);
				}
				Assert.AreEqual(filesToLocate.Length, results.Count());
				Assert.AreEqual(lastWriteTime, results.First().LastModificationTime);
			}
		}

		[Test]
		public void GetDownloadUrlAndPublicationDate()
		{
			var html = File.ReadAllText(DailyTariffTestFiles.NewDailyTaricWebPageHtmlFile);
			var result = DailyTariffFactory.GetDownloadUrlAndPublicationDate(html);
			Assert.AreEqual("https://ec.europa.eu/taxation_customs/dds2/taric/taric_management.jsp?publicationDate=2022-07-27&message=extract", result.Item1);
			Assert.AreEqual(new DateTime(2022, 7, 27), result.Item2);
		}

		[Test]
		public void GetAndMoveExtractedDailyFile()
		{
			var dailyTariffFactoryForTest = new DailyTariffFactoryForTest(new[] { new RefCusTariff() });
			var tempPath = Path.Combine(Path.GetTempPath(), "{7BB0E3C9-07B4-4F54-8093-EBF87B18719D}");
			if (!Directory.Exists(tempPath))
			{
				Directory.CreateDirectory(tempPath);
			}
			ApplicationConfig.SetDownloadsFolder(tempPath);
			var files = new[] { "Add_Codes_20230605_1925.xlsx", "Goods_Nomenclature_20230605_1925.xlsx", "Measures_20230605_1925.xlsx", "Measures.txt" };
			files = files.Select(x => Path.Combine(tempPath, x)).ToArray();
			foreach (var file in files)
			{
				File.WriteAllText(Path.Combine(tempPath, file), "");
			}
			var dailyFile = dailyTariffFactoryForTest.MoveExtractedDailyFile(files);
			Assert.True(File.Exists(dailyFile));
			Assert.AreEqual(Path.Combine(tempPath, ApplicationConfig.DailyFileName), dailyFile);
			Assert.AreEqual(1, Directory.GetFiles(tempPath).Length);

			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
		}

		[SetUp]
		public void SetUp()
		{
			dailyTariffFactory = new DailyTariffFactory(new[] { new RefCusTariff() });
		}
	}

	class DailyTariffFactoryForTest : DailyTariffFactory
	{
		public DailyTariffFactoryForTest(ICollection<RefCusTariff> tariffCollection, string downloadFolderPath = "") : base(tariffCollection, downloadFolderPath)
		{
		}

		public new string MoveExtractedDailyFile(string[] files)
		{
			return DailyTariffFactory.MoveExtractedDailyFile(files);
		}
	}
}
