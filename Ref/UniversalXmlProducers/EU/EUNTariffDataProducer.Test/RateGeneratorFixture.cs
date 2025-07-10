using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class RateGeneratorFixture
	{
		[Test]
		public void ConvertSetsExclusionTradeGroup()
		{
			var formulaExtractor = new Mock<IFormulaExtractor>();
			var rateGenerator = new RateGenerator(formulaExtractor.Object);
			var reductionIndicator = string.Empty;
			var rateRecord = new GroupedRateRecord("103", null, null, "0.000 %", new DateTime(2008, 12, 29, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 00), reductionIndicator, "DTY");
			var tradeGroups = new[] { "1033" };

			formulaExtractor.Setup(x => x.GetFormula(rateRecord.Rate, rateRecord.RateCode, rateRecord.ReductionIndicator)).Returns(new List<FormulaExtractionResult>() { new FormulaExtractionResult("0", "A00") });

			var exclusion = new[]
			{
				new RawMeasureExclusionRecord("0101000000", null, null, new DateTime(2008,12,29, 0, 0, 0), new DateTime(2079,6,6, 23, 59,00), "CARIFORUM",
					"Tariff preference", "1033", "103", "HT")
			};

			var results = rateGenerator.Convert(rateRecord, tradeGroups, exclusion, null).ToList();
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count);

			Assert.True(results.Any(x => x.ZZ2_ZZS_NKPreference == "103"));

			Assert.True(results.All(x => x.RefCusApplicabilities != null));
			Assert.True(results.All(x => x.RefCusApplicabilities.All(y => y.RefCusExcludedTradeGroups != null)));
			Assert.True(results.All(x => x.RefCusApplicabilities.All(y => y.RefCusExcludedTradeGroups.All(z => z.ZZC_ZZA_NKTradeGroup == "HT"))));
		}

		[Test]
		public void ApplyMeasuresConditionActionCode07LogicInFormula()
		{
			var formulaExtractor = new Mock<IFormulaExtractor>();
			var rateGenerator = new RateGenerator(formulaExtractor.Object);
			var reductionIndicator = string.Empty;
			var rateRecord = new GroupedRateRecord("552", null, null, "any", new DateTime(2008, 12, 29, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 00), reductionIndicator, "DTY");
			var tradeGroups = new[] { "1011" };
			formulaExtractor.Setup(x => x.GetFormula(rateRecord.Rate, rateRecord.RateCode, rateRecord.ReductionIndicator)).Returns(new List<FormulaExtractionResult>() { new FormulaExtractionResult("VFD * 0.300", "DTY") });

			var measureConditions = new[]
			{
				new RawMeasureConditionRecord("0101000000", null, null, rateRecord.StartDate, rateRecord.EndDate, "1011", "552", "ANY", "D019", "", "", "", "07", "")
			};

			var result = rateGenerator.Convert(rateRecord, tradeGroups, Enumerable.Empty<IRawMeasureExclusionRecord>(), measureConditions).FirstOrDefault();
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ZZ2_RateFormula, Is.EqualTo(@"if(has(""CERT"", ""D019""),0,VFD * 0.300)"));

			//should not change the formula
			measureConditions = new[]
			{
				new RawMeasureConditionRecord("0101000000", null, null, rateRecord.StartDate, rateRecord.EndDate, "1011", "552", "ANY", "D019", "", "", "", "09", "")
			};

			result = rateGenerator.Convert(rateRecord, tradeGroups, null, measureConditions).FirstOrDefault();
			Assert.That(result, Is.Not.Null);
			Assert.That(result.ZZ2_RateFormula, Is.EqualTo(@"VFD * 0.300"));
		}

		[Test]
		public void ConvertSetsExclusionTradeGroupOnApplicableTradeGroup()
		{
			var formulaExtractor = new Mock<IFormulaExtractor>();
			var rateGenerator = new RateGenerator(formulaExtractor.Object);
			var reductionIndicator = string.Empty;
			var rateRecord = new GroupedRateRecord("103", null, null, "0.000 %", new DateTime(2008, 12, 29, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 00), reductionIndicator, "DTY");
			var tradeGroups = new[] { "1033", "1011" };
			formulaExtractor.Setup(x => x.GetFormula(rateRecord.Rate, rateRecord.RateCode, rateRecord.ReductionIndicator)).Returns(new List<FormulaExtractionResult>() { new FormulaExtractionResult("0", "A00") });

			var exclusion = new[]
			{
				new RawMeasureExclusionRecord("0101000000", null, null, new DateTime(2008,12,29, 0, 0, 0), new DateTime(2079,6,6, 23, 59,00), "CARIFORUM",
					"Tariff preference", "1033", "103", "HT")
			};

			var results = rateGenerator.Convert(rateRecord, tradeGroups, exclusion, null).ToList();
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count);

			Assert.True(results.All(x => x.ZZ2_ZZS_NKPreference == "103"));

			Assert.True(results.All(x => x.RefCusApplicabilities != null));
			Assert.True(results.All(x => x.RefCusApplicabilities.All(y => y.RefCusExcludedTradeGroups != null)));

			Assert.True(results.All(x => x.RefCusApplicabilities.First(y => y.ZZT_ZZA_NKTradeGroup == "1011").RefCusExcludedTradeGroups.Length == 0));
			Assert.True(results.All(x => x.RefCusApplicabilities.First(y => y.ZZT_ZZA_NKTradeGroup == "1033").RefCusExcludedTradeGroups.Length == 1));
			Assert.True(results.All(x => x.RefCusApplicabilities.First(y => y.ZZT_ZZA_NKTradeGroup == "1033").RefCusExcludedTradeGroups.All(z => z.ZZC_ZZA_NKTradeGroup == "HT")));
		}
	}
}
