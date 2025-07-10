using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class TariffDataProducerFixture
	{
		[Test]
		public void LoadAllTariffs()
		{
			ApplicationConfig.ConfigEnvironment();

			var tariffGenerator = new Mock<ITariffGenerator>();
			var tariffMerger = new Mock<ITariffMerger>();

			var rawRecord = new ImportRawRecord();
			rawRecord.UomRecords = rawUomRecords;
			rawRecord.RateRecords = rawRateRecords;
			rawRecord.MeasureExclusionRecords = rawMeasureExclusionRecords;
			rawRecord.MeasureConditionRecords = rawMeasureConditionRecords;

			tariffGenerator.Setup(x => x.GenerateRawTariff("0100000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff1);
			tariffGenerator.Setup(x => x.GenerateRawTariff("0101000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff2);
			tariffGenerator.Setup(x => x.GenerateRawTariff("0101210000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff3);
			tariffGenerator.Setup(x => x.GenerateRawTariff("0101290000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff4);
			tariffGenerator.Setup(x => x.GenerateRawTariffWithonlyMeasureConditions("0101291000", It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff5);

			var rawTariffs = new[] { rawTariff1, rawTariff2, rawTariff3, rawTariff4, rawTariff5 };
			tariffMerger.Setup(x => x.ConvertToActualTariffs(rawTariffs, null, It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(new[] { actualTariff1, actualTariff2, actualTariff3 });

			var tariffDataProducer = new TariffDataProducer(rawRecord, tariffGenerator.Object, tariffMerger.Object);
			var result = tariffDataProducer.LoadAllTariffRates().ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Count);

			tariffGenerator.Verify(x => x.GenerateRawTariff("0100000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
			tariffGenerator.Verify(x => x.GenerateRawTariff("0101000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
			tariffGenerator.Verify(x => x.GenerateRawTariff("0101210000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
			tariffGenerator.Verify(x => x.GenerateRawTariff("0101290000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.Is<IEnumerable<IRawRateRecord>>(y => y.Count() == 1 && y.Contains(rawUomRecord1))), Times.Once);

			tariffMerger.Verify(x => x.ConvertToActualTariffs(rawTariffs, null, It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
		}

		[Test]
		public void TestMergeImportTariffsWithEUNTarrifs()
		{
			ApplicationConfig.ConfigEnvironment();

			var tariffGenerator = new Mock<ITariffGenerator>();
			var tariffMerger = new Mock<ITariffMerger>();

			tariffGenerator.Setup(x => x.GenerateRawTariff("0100000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff1);
			tariffGenerator.Setup(x => x.GenerateRawTariff("0101000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff2);
			tariffGenerator.Setup(x => x.GenerateRawTariff("0101210000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff3);
			tariffGenerator.Setup(x => x.GenerateRawTariff("0101290000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff4);
			tariffGenerator.Setup(x => x.GenerateRawTariffWithonlyMeasureConditions("0101291000", It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(rawTariff5);

			var rawTariffs = new[] { rawTariff1, rawTariff2, rawTariff3, rawTariff4, rawTariff5 };
			tariffMerger.Setup(x => x.ConvertToActualTariffs(rawTariffs, null, It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(new[] { actualTariff1, actualTariff2, actualTariff3 });

			var rawRecord = new ImportRawRecord();
			rawRecord.UomRecords = rawUomRecords;
			rawRecord.RateRecords = rawRateRecords;
			rawRecord.MeasureExclusionRecords = rawMeasureExclusionRecords;
			rawRecord.MeasureConditionRecords = rawMeasureConditionRecords;

			var tariffDataProducer = new TariffDataProducer(rawRecord, tariffGenerator.Object, tariffMerger.Object);
			var result = tariffDataProducer.LoadAllTariffRates().ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Count);

			tariffGenerator.Verify(x => x.GenerateRawTariff("0100000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
			tariffGenerator.Verify(x => x.GenerateRawTariff("0101000000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
			tariffGenerator.Verify(x => x.GenerateRawTariff("0101210000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
			tariffGenerator.Verify(x => x.GenerateRawTariff("0101290000", It.IsAny<IEnumerable<IRawRateRecord>>(), It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.Is<IEnumerable<IRawRateRecord>>(y => y.Count() == 1 && y.Contains(rawUomRecord1))), Times.Once);

			tariffMerger.Verify(x => x.ConvertToActualTariffs(rawTariffs, null, It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Once);
		}

		[Test]
		public void PassingParentTariffUOMsToChildren()
		{
			var rawUomRecord3 = new RawRateRecord("5208000000", null, null, new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "ddesc", "ddesc2", "", "1011", "109", "MTK", "");
			var rawRateRecord6 = new RawRateRecord("5208129600", null, null, new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "desc", "desc2", "", "1011", "482", "Cond:  R 0.131/KGM(10):; R 0.101/KGM(28):; R 0.000/KGM(10):", "");
			var rawMeasureConditionRecords1 = new RawRateToRawMeasureConditionConverter().Convert(rawRateRecord6);
			var tariffGenerator = new Mock<ITariffGenerator>();
			var tariffMerger = new Mock<ITariffMerger>();

			var rawRecord = new ImportRawRecord();
			rawRecord.UomRecords = new[] { rawUomRecord3 };
			rawRecord.RateRecords = new List<IRawRateRecord>();
			rawRecord.MeasureExclusionRecords = new List<IRawMeasureExclusionRecord>();
			rawRecord.MeasureConditionRecords = rawMeasureConditionRecords1;

			var tariffDataProducer = new TariffDataProducer(rawRecord, tariffGenerator.Object, tariffMerger.Object);
			tariffDataProducer.LoadTariffRates(new[] { new WebTariffHeader("5208129620", "Desc") });
			tariffGenerator.Verify(x => x.GenerateRawTariffWithonlyMeasureConditions("5208129600", It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(),
				It.IsAny<IEnumerable<IRawMeasureExclusionRecord>>(), new[] { rawUomRecord3 }));
		}

		#region Setup

		IRawRateRecord rawRateRecord1, rawRateRecord2, rawRateRecord3, rawRateRecord4, rawRateRecord5;
		IRawRateRecord rawUomRecord1, rawUomRecord2;
		IRawMeasureExclusionRecord rawMeasureExclusionRecord1, rawMeasureExclusionRecord2;
		IRawMeasureConditionRecord rawMeasureConditionRecord1, rawMeasureConditionRecord2;
		IEnumerable<IRawRateRecord> rawRateRecords;
		IEnumerable<IRawMeasureExclusionRecord> rawMeasureExclusionRecords;
		IEnumerable<IRawMeasureConditionRecord> rawMeasureConditionRecords;
		IEnumerable<IRawRateRecord> rawUomRecords;
		RefCusTariff rawTariff1, rawTariff2, rawTariff3, rawTariff4, rawTariff5;
		RefCusTariff actualTariff1, actualTariff2, actualTariff3;

		[SetUp]
		protected void Setup()
		{
			rawRateRecord1 = new RawRateRecord("0100000000", null, null, new DateTime(1998, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "desc", "desc2", "", "AD", "103", "0.000 %", "");
			rawRateRecord2 = new RawRateRecord("0101000000", null, null, new DateTime(1998, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "desc", "desc2", "", "CH", "103", "0.000 %", "");
			rawRateRecord3 = new RawRateRecord("0101210000", "C2510", null, new DateTime(2005, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "desc", "desc2", "", "LI", "103", "0.000 %", "");
			rawRateRecord4 = new RawRateRecord("0101290000", "C2510", null, new DateTime(2005, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "desc", "desc2", "", "IS", "103", "0.000 %", "");
			rawRateRecord5 = new RawRateRecord("0101290000", null, "99180", new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "desc", "desc2", "", "1011", "103", "0.000 %", "");
			rawUomRecord1 = new RawRateRecord("0101290000", null, "99180", new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "desc", "desc2", "", "1011", "109", "KGM E", "");
			rawUomRecord2 = new RawRateRecord("2900000000", null, "99190", new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "ddesc", "ddesc2", "", "1011", "109", "MTK E", "");
			rawRateRecords = new[] { rawRateRecord1, rawRateRecord2, rawRateRecord3, rawRateRecord4, rawRateRecord5 };
			rawUomRecords = new[] { rawUomRecord1, rawUomRecord2 };
			rawMeasureExclusionRecord1 = new RawMeasureExclusionRecord("0101290000", null, "99180", new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "desc", "desc2", "1011", "103", "GB");
			rawMeasureExclusionRecord2 = new RawMeasureExclusionRecord("0101290000", null, "99180", new DateTime(2018, 1, 1, 0, 0, 0), new DateTime(2018, 6, 6, 23, 59, 0), "desc", "desc2", "1011", "103", "IT");
			rawMeasureExclusionRecords = new[] { rawMeasureExclusionRecord1, rawMeasureExclusionRecord2 };

			rawMeasureConditionRecord1 = new RawMeasureConditionRecord("0101290000", null, null, new DateTime(2017, 09, 21, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "1006", "142", "B", "U088", null, null, null, "27", null);
			rawMeasureConditionRecord2 = new RawMeasureConditionRecord("0101291000", null, null, new DateTime(2012, 01, 01, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "1011", "105", "B", "N990", null, null, null, "27", null);
			rawMeasureConditionRecords = new[] { rawMeasureConditionRecord1, rawMeasureConditionRecord2 };

			rawTariff1 = new RefCusTariff { ZZ1_TariffCode = "0100000000" };
			rawTariff2 = new RefCusTariff { ZZ1_TariffCode = "0101000000" };
			rawTariff3 = new RefCusTariff { ZZ1_TariffCode = "0101210000" };
			rawTariff4 = new RefCusTariff { ZZ1_TariffCode = "0101290000" };
			rawTariff5 = new RefCusTariff { ZZ1_TariffCode = "0101291000" };

			actualTariff1 = new RefCusTariff { ZZ1_TariffCode = "0101210000" };
			actualTariff2 = new RefCusTariff { ZZ1_TariffCode = "0101291000" };
			actualTariff3 = new RefCusTariff { ZZ1_TariffCode = "0101299000" };
		}

		#endregion
	}
}
