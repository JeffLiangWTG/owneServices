using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class TariffGeneratorFixture
	{
		[Test]
		public void GenerateRawTariffs()
		{
			var reductionIndicator = string.Empty;
			var rawRateRecords = new[]
			{
				new RawRateRecord("0102030405", null, null, new DateTime(1998, 1, 1, 0,0,0), new DateTime(2079, 6,6, 23, 59, 0), "desc","desc2", "", "AD", "103", "0.000 %", reductionIndicator),
				new RawRateRecord("0102030405", null, null, new DateTime(1998, 1, 1, 0,0,0), new DateTime(2079, 6,6, 23, 59, 0), "desc","desc2", "", "CH", "103", "96.90 EUR DTN", reductionIndicator),
				new RawRateRecord("0102030405", "C2510", null, new DateTime(2005, 1, 1, 0,0,0), new DateTime(2079, 6,6, 23, 59, 0), "desc","desc2", "", "LI", "103", "0.000 %" ,reductionIndicator),
				new RawRateRecord("0102030405", "C2510", null, new DateTime(2005, 1, 1, 0,0,0), new DateTime(2079, 6,6, 23, 59, 0), "desc","desc2", "", "IS", "103", "0.000 %", reductionIndicator),
				new RawRateRecord("0102030405", null, "99180", new DateTime(2018, 1, 1, 0,0,0), new DateTime(2018, 6,6, 23, 59, 0), "desc","desc2", "", "1011", "103", "0.000 %", reductionIndicator),
			};

			var rawMeasureExclusionRecords = new[]
			{
				new RawMeasureExclusionRecord("0102030405", null, "99180", new DateTime(2018, 1, 1, 0,0,0), new DateTime(2018, 6,6, 23, 59, 0), "desc", "desc2", "1011", "103", "GB"),
				new RawMeasureExclusionRecord("0102030405", null, "99180", new DateTime(2018, 1, 1, 0,0,0), new DateTime(2018, 6,6, 23, 59, 0), "desc", "desc2", "1011", "103", "IT"),
			};

			var rawMeasureConditionRecords = new[]
			{
				new RawMeasureConditionRecord("0102030405", null, null, new DateTime(2017, 09, 21, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "1006", "142", "B", "U088", null, null, null, "27", null),
				new RawMeasureConditionRecord("0102030405", null, null, new DateTime(2012, 01, 01, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), "1011", "105", "B", "N990", null, null, null, "27", null),
			};

			var rawUomRecords = new RawRateRecord[]
			{
				new RawRateRecord("0102030400", null, "99180", new DateTime(2018, 1, 1, 0,0,0), new DateTime(2018, 6,6, 23, 59, 0), "desc","desc2", "", "1011", "109", "NAR", reductionIndicator),
			};

			var formulaExtractor = new Mock<IFormulaExtractor>();
			formulaExtractor.Setup(x => x.GetFormula(rawRateRecords[1].Rate, rawRateRecords[1].RateCode, reductionIndicator)).Returns(new[] { new FormulaExtractionResult("96.90 * [DTN]", "A00") });
			formulaExtractor.Setup(x => x.GetFormula(rawRateRecords[0].Rate, rawRateRecords[0].RateCode, reductionIndicator)).Returns(new[] { new FormulaExtractionResult("0", "A00") });
			var rateGenerator = new RateGenerator(formulaExtractor.Object);
			var conditionGenerator = new Mock<IConditionGenerator>();
			conditionGenerator.Setup(x => x.Convert(It.IsAny<IGroupedMeasureConditionKey>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IRawRateRecord>>())).Returns(new RefCusCondition());
			var tariffGenerator = new TariffGenerator(rateGenerator, conditionGenerator.Object);
			var tariff = tariffGenerator.GenerateRawTariff("0102030405", rawRateRecords, rawMeasureExclusionRecords, rawMeasureConditionRecords, rawUomRecords);

			Assert.IsNotNull(tariff);
			Assert.AreEqual(tariff.ZZ1_TariffCode, "0102030405");
			Assert.IsNotNull(tariff.RefCusRates);
			Assert.AreEqual(4, tariff.RefCusRates.Length);

			Assert.AreEqual(1, tariff.RefCusRates[0].RefCusApplicabilities.Length);
			Assert.IsNull(tariff.RefCusRates[0].RefCusApplicabilities[0].RefCusExcludedTradeGroups);
			Assert.IsNull(tariff.RefCusRates[0].RefCusRateUOMs);

			Assert.AreEqual(1, tariff.RefCusRates[1].RefCusApplicabilities.Length);
			Assert.IsNull(tariff.RefCusRates[1].RefCusApplicabilities[0].RefCusExcludedTradeGroups);
			Assert.IsNotNull(tariff.RefCusRates[1]);
			Assert.AreEqual(1, tariff.RefCusRates[1].RefCusRateUOMs.Length);
			Assert.AreEqual("DTN", tariff.RefCusRates[1].RefCusRateUOMs[0].ZXG_UOM);

			Assert.AreEqual(1, tariff.RefCusRates[3].RefCusApplicabilities.Length);
			Assert.IsNotNull(tariff.RefCusRates[3].RefCusApplicabilities[0].RefCusExcludedTradeGroups);
			Assert.AreEqual(2, tariff.RefCusRates[3].RefCusApplicabilities[0].RefCusExcludedTradeGroups.Length);

			Assert.IsNull(tariff.RefCusTariffUOMs);

			conditionGenerator.Verify(x => x.Convert(It.IsAny<IGroupedMeasureConditionKey>(), It.IsAny<IEnumerable<IRawMeasureConditionRecord>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IRawRateRecord>>()), Times.Exactly(2));
		}
	}
}
