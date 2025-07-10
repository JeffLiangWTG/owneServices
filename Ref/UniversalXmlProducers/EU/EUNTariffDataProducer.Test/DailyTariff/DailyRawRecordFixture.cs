using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	class DailyRawRecordFixture
	{
		[Test]
		public void Parse()
		{
			_seMeasureParserMock.Setup(x => x.CanDownload()).Returns(true);
			var webFiles = new List<WebFileInfo>()
			{
				DailyTariffTestFiles.MonthlyDutiesImportWebFileInfo,
				DailyTariffTestFiles.MonthlyMeasuresConditionWebFileInfo,
				DailyTariffTestFiles.MonthlyMeasuresExclusionWebFileInfo,
				DailyTariffTestFiles.SampleMeasuresWebFileInfo
			};
			defaultRawRecord.Parse(webFiles);

			_dailyTariffRateParserMock.Verify(x => x.Parse(It.IsAny<string>()), Times.Once);
			_seMeasureParserMock.Verify(x => x.Parse(null, It.IsAny<List<IRawMeasureExclusionRecord>>()), Times.Never);
		}

		[Test]
		public void SEParse()
		{
			_seMeasureParserMock.Setup(x => x.CanDownload()).Returns(true);
			defaultRawRecord.ParseSEFileAndAttachTaricDailyRecords();

			_seMeasureParserMock.Verify(x => x.CanDownload(), Times.Once);
			_seMeasureParserMock.Verify(x => x.Parse(It.IsAny<List<IRawMeasureConditionRecord>>(), It.IsAny<List<IRawMeasureExclusionRecord>>()), Times.Once);
		}

		[Test]
		public void SEParseDoNotParseWhenCannotDownload()
		{
			_seMeasureParserMock.Setup(x => x.CanDownload()).Returns(false);
			defaultRawRecord.ParseSEFileAndAttachTaricDailyRecords();

			_seMeasureParserMock.Verify(x => x.CanDownload(), Times.Once);
			_seMeasureParserMock.Verify(x => x.Parse(It.IsAny<List<IRawMeasureConditionRecord>>(), It.IsAny<List<IRawMeasureExclusionRecord>>()), Times.Never);
		}

		Mock<ISEMeasureParser> _seMeasureParserMock;
		Mock<IDailyTariffRateParser> _dailyTariffRateParserMock;
		IRawRecord defaultRawRecord;

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
			_seMeasureParserMock = new Mock<ISEMeasureParser>();
			_dailyTariffRateParserMock = new Mock<IDailyTariffRateParser>();
			defaultRawRecord = new ImportRawRecord(_dailyTariffRateParserMock.Object, _seMeasureParserMock.Object);
		}
	}
}
