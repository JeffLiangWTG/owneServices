using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.SE
{
	[TestFixture]
	class SEMeasureToRawRecordConverterFixture
	{
		[Test]
		public void AddRawMeasureConditionRecordList()
		{
			var conditionRecords = Enumerable.Empty<IRawMeasureConditionRecord>().ToList();
			var exclusionRecords = Enumerable.Empty<IRawMeasureExclusionRecord>().ToList();
			var canDownload = _converter.CanDownload();
			Assert.True(canDownload);
			_converter.Parse(conditionRecords, exclusionRecords);
			Assert.AreEqual(1, conditionRecords.Count);
			var record = conditionRecords.First();
			Assert.AreEqual("70012020", record.TariffHeader);
			Assert.AreEqual("103", record.MeasureTypeId);
			Assert.AreEqual("1011", record.TradeGroup);
			Assert.AreEqual("12345", record.OrderNumber);
			Assert.AreEqual(new DateTime(2010, 1, 1), record.StartDate);
			Assert.AreEqual(new DateTime(2030, 12, 31, 23, 59, 0), record.EndDate);
			Assert.AreEqual("N999", record.CertificateTypeCode);
			Assert.AreEqual("27", record.MeasureAction);
			Assert.AreEqual("10", record.ConditionAmount);
			Assert.AreEqual("EUR", record.MonetaryUnitCode);
			Assert.AreEqual("NAR", record.MeasureUnit);
			Assert.AreEqual("B", record.MeasureConditionCode);
		}

		[Test]
		public void AddRawMeasureConditionRecordListAvoidsRecordsWithDutySpecified()
		{
			var measureList = new List<measure>() { new measure {
				geographicalAreaId = "1011",
				measureType = "105",
				goodsNomenclatureCode = "70011010",
				quotaOrderNumber = "12345",
				dateStart = new DateTime(2010,1,1),
				dateEnd = new DateTime(2030,12,31,23,59,0),
				measureCondition = new []
					{
						new measureCondition
						{
							conditionCodeId = "A",
							actionCode = "CD",
							certificateCode = "B",
							certificateType = "AAA",
							measureConditionComponent = new measureConditionComponent[]
							{
								new measureConditionComponent { dutyAmount = 10, dutyAmountSpecified = true }
							}
						},
						new measureCondition
						{
							conditionCodeId = "A",
							actionCode = "CD",
							certificateCode = "C",
							certificateType = "AAA",
							measureConditionComponent = new measureConditionComponent[]
							{
								new measureConditionComponent { national = 0 }
							}
						}
					}
				}
			};
			_seMeasureDownloaderMock.Setup(x => x.DownloadLatestIncrementalAndExtract(It.IsAny<string>(), It.IsAny<string>())).Returns(() => measureList.ToArray());

			var conditionRecords = Enumerable.Empty<IRawMeasureConditionRecord>().ToList();
			var exclusionRecords = Enumerable.Empty<IRawMeasureExclusionRecord>().ToList();
			var canDownload = _converter.CanDownload();
			Assert.True(canDownload);
			_converter.Parse(conditionRecords, exclusionRecords);
			Assert.AreEqual(1, conditionRecords.Count);

			Assert.That(conditionRecords.First().CertificateTypeCode, Is.EqualTo("AAAC"));
		}

		[Test]
		public void AddRawMeasureExclusionRecordList()
		{
			var exclusionRecords = Enumerable.Empty<IRawMeasureExclusionRecord>().ToList();
			var conditionRecords = Enumerable.Empty<IRawMeasureConditionRecord>().ToList();
			_converter.CanDownload();
			_converter.Parse(conditionRecords, exclusionRecords);
			Assert.AreEqual(2, exclusionRecords.Count);
			Assert.AreEqual("AU", exclusionRecords.First().ExcludedTradeGroup);
			Assert.AreEqual("70011010", exclusionRecords.First().TariffHeader);
			Assert.AreEqual("105", exclusionRecords.First().MeasureTypeId);
			Assert.AreEqual("1011", exclusionRecords.First().TradeGroup);
			Assert.AreEqual("12345", exclusionRecords.First().OrderNumber);
			Assert.AreEqual(new DateTime(2010, 1, 1), exclusionRecords.First().StartDate);
			Assert.AreEqual(new DateTime(2030, 12, 31, 23, 59, 0), exclusionRecords.First().EndDate);
			Assert.AreEqual("ZA", exclusionRecords.Skip(1).First().ExcludedTradeGroup);
		}

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
			_seMeasureDownloaderMock = new Mock<ISEMeasureDownloader>();
			_seMeasureDownloaderMock.Setup(x => x.DownloadLatestIncrementalAndExtract(It.IsAny<string>(), It.IsAny<string>())).Returns(() => MeasureList.ToArray());
			_converter = new SEMeasureParser(_seMeasureDownloaderMock.Object);
		}

		Mock<ISEMeasureDownloader> _seMeasureDownloaderMock;
		SEMeasureParser _converter;

		List<measure> MeasureList => new List<measure>()
		{
			new measure() {
				geographicalAreaId = "1011",
				measureType = "105",
				goodsNomenclatureCode = "70011010",
				quotaOrderNumber = "12345",
				dateStart = new DateTime(2010,1,1),
				dateEnd = new DateTime(2030,12,31,23,59,0),
				measureExcludedGeographicalArea = new []
				{
					new measureExcludedGeographicalArea { geographicalAreaId = "AU" },
					new measureExcludedGeographicalArea { geographicalAreaId = "ZA" }
				}
			},
			new measure()
			{
				geographicalAreaId = "1011",
				measureType = "103",
				goodsNomenclatureCode = "70012020",
				quotaOrderNumber = "12345",
				dateStart = new DateTime(2010,1,1),
				dateEnd = new DateTime(2030,12,31,23,59,0),
				measureCondition = new []
				{
					new measureCondition
					{
						certificateCode = "999",
						certificateType = "N",
						actionCode = "27",
						conditionCodeId = "B",
						dutyAmount = 10,
						monetaryUnitCode = "EUR",
						measurementUnitCode = "NAR"
					}
				}
			}
		};
	}
}
