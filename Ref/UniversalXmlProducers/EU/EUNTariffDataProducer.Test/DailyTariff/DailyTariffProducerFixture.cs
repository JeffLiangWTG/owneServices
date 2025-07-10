using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	class DailyTariffProducerFixture
	{
		[Test]
		public void SimpleExecutionSingleTariffDaily()
		{
			var tariffHeaders = new List<string>() { "7210708011" };
			_dailyRawRecordMock.Setup(x => x.GetDailyTariffHeadersToProcess()).Returns(tariffHeaders);
			_dailyTariffFactoryMock.Setup(x => x.TariffCollectionFromNomenclature).Returns(new[] { new RefCusTariff
			{
				ZZ1_TariffCode = "7210708011",
				ZZ1_Description = "Desc"
			} });

			_dailyTariffDataProducerMock.Setup(x => x.LoadTariffRates(It.IsAny<IEnumerable<IWebTariffHeader>>())).Returns(new[] {
				new RefCusTariff
				{
					ZZ1_TariffCode = "7210708011",
					ZZ1_Description = "Desc"
				}
			});

			_dailyTariffProducer.Run();
			_xmlProducerMock.Verify(x => x.InitializeWriter(It.IsAny<DateTime>(), null, UpdateType.Partial), Times.Once);
			_xmlProducerMock.Verify(x => x.ExportToXml(It.IsAny<IEnumerable<RefCusTariff>>(), null), Times.Once);
		}

		[Test]
		public void SimpleExecutionMultipleTariffsDaily()
		{
			var tariffHeaders = new List<string>() { "7210708011", "2020" };
			_dailyRawRecordMock.Setup(x => x.GetDailyTariffHeadersToProcess()).Returns(tariffHeaders);
			_dailyTariffFactoryMock.SetupGet(x => x.TariffCollectionFromNomenclature).Returns(new[] {
				new RefCusTariff
				{
					ZZ1_TariffCode = "7210708011",
					ZZ1_Description = "Desc"
				},
				new RefCusTariff
				{
					ZZ1_TariffCode = "2020",
					ZZ1_Description = "Desc2"
				}
			});

			_dailyTariffDataProducerMock.Setup(x => x.LoadTariffRates(It.IsAny<IEnumerable<IWebTariffHeader>>())).Returns(new[] {
				new RefCusTariff
				{
					ZZ1_TariffCode = "7210708011",
					ZZ1_Description = "Desc"
				},
				new RefCusTariff
				{
					ZZ1_TariffCode = "2020",
					ZZ1_Description = "Desc2"
				}
			});

			_dailyTariffProducer.Run();
			_xmlProducerMock.Verify(x => x.InitializeWriter(It.IsAny<DateTime>(), null, UpdateType.Partial), Times.Once);
			_xmlProducerMock.Verify(x => x.ExportToXml(It.IsAny<IEnumerable<RefCusTariff>>(), null), Times.Once);
		}

		Mock<IFileDownloaderWrapper> _fileDownloaderWrapperMock;
		Mock<IRawRecord> _dailyRawRecordMock;
		Mock<IDailyTariffFactory> _dailyTariffFactoryMock;
		DailyTariffProducer _dailyTariffProducer;
		Mock<ITariffDataProducer> _dailyTariffDataProducerMock;
		Mock<IXmlProducer<RefCusTariff>> _xmlProducerMock;
		Mock<IWebDriverHelper> _webDriverHelperMock;

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
			var downloadedFiles = new[]
			{
				DailyTariffTestFiles.SampleMeasuresPath
			};
			_webDriverHelperMock = new Mock<IWebDriverHelper>();
			_xmlProducerMock = new Mock<IXmlProducer<RefCusTariff>>();
			_dailyTariffDataProducerMock = new Mock<ITariffDataProducer>();

			_dailyRawRecordMock = new Mock<IRawRecord>();
			_fileDownloaderWrapperMock = new Mock<IFileDownloaderWrapper>();
			_fileDownloaderWrapperMock.Setup(x => x.DownloadAndExtract(It.IsAny<string>(), It.IsAny<string>(), null)).Returns(downloadedFiles);
			_fileDownloaderWrapperMock.Setup(x => x.GetFileInfo(DailyTariffTestFiles.SampleMeasuresPath)).Returns(new FileInfo(DailyTariffTestFiles.SampleMeasuresPath) { CreationTime = DateTime.UtcNow });

			var webFileInfoMockDutiesImport = new Mock<IWebFileInfo>();
			webFileInfoMockDutiesImport.Setup(x => x.FileName).Returns(DailyTariffTestFiles.MonthlyDutiesImportPath);
			var webFileInfoMockMeasureConditions = new Mock<IWebFileInfo>();
			webFileInfoMockMeasureConditions.Setup(x => x.FileName).Returns(DailyTariffTestFiles.MonthlyMeasuresConditionPath);
			var webFileInfoMockMeasureExclusions = new Mock<IWebFileInfo>();
			webFileInfoMockMeasureExclusions.Setup(x => x.FileName).Returns(DailyTariffTestFiles.MonthlyMeasuresExclusionPath);

			var fileInfos = new IWebFileInfo[] { webFileInfoMockDutiesImport.Object, webFileInfoMockMeasureConditions.Object, webFileInfoMockMeasureExclusions.Object };

			var monthlyProducerMock = new Mock<IProducer>();
			monthlyProducerMock.Setup(x => x.LocateWebFiles()).Returns(fileInfos);
			monthlyProducerMock.Setup(x => x.DownloadFiles(fileInfos)).Returns(true);

			_dailyTariffFactoryMock = new Mock<IDailyTariffFactory>();
			_dailyTariffFactoryMock.Setup(x => x.LocateWebFiles(It.IsAny<IWebDriverHelper>())).Returns(fileInfos);
			_dailyTariffFactoryMock.Setup(x => x.GetOrDownloadMonthlyFiles(monthlyProducerMock.Object)).Returns(fileInfos);
			_dailyTariffFactoryMock.Setup(x => x.GetRawRecord(It.IsAny<IDailyTariffRateParser>(), It.IsAny<ISEMeasureParser>())).Returns(_dailyRawRecordMock.Object);
			_dailyTariffFactoryMock.Setup(x => x.GetDailyTariffDataProducer(It.IsAny<IRawRecord>(), It.IsAny<ITariffGenerator>(), It.IsAny<ITariffMerger>())).Returns(_dailyTariffDataProducerMock.Object);
			_dailyTariffFactoryMock.Setup(x => x.GetXmlProducer()).Returns(_xmlProducerMock.Object);
			_dailyTariffFactoryMock.Setup(x => x.GetWebDriverHelper()).Returns(_webDriverHelperMock.Object);

			var seMeasureParserMock = new Mock<ISEMeasureParser>();
			_dailyTariffFactoryMock.Setup(x => x.GetSEMeasureParser()).Returns(seMeasureParserMock.Object);
			var seXmlProducer = new Mock<IXmlProducer<RefCusTariff>>();
			_dailyTariffFactoryMock.Setup(x => x.GetSEXmlProducer()).Returns(seXmlProducer.Object);

			_dailyTariffProducer = new DailyTariffProducer(_dailyTariffFactoryMock.Object);
		}
	}
}
