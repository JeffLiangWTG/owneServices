using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class ScrappedRecordParserFixture
	{
		[Test]
		public void GetRefCusRates_DoNotCreateRateIfFormulaIsEmpty()
		{
			var scrappedRecord = new Mock<IScrappedRecord>();
			var parser = new ScrappedRecordParser(new Mock<IDataLookup>().Object, new Mock<IDataLookup>().Object,
				new Mock<IRateCodeDataLookup>().Object, DateTime.Today);
			scrappedRecord.Setup(x => x.Measure).Returns(new MeasureData("AAAA", new[] { "BBB" }, string.Empty));
			var tariff = new RefCusTariff("0201");
			Assert.IsNull(parser.GetRefCusRates(scrappedRecord.Object, string.Empty, tariff, new DefaultRateGenerationStrategy()));
			Assert.AreEqual(0, tariff.CusRates.Count);
		}

		[TestCase("9999 x", "9999 x", TestName = "GetRefCusRates_WhenFormulaDoesNotContainEuroTokenNorDecimalParsableValue")]
		[TestCase("15.98256 EURO/1000 kg", "15.98256 * [TNE]", TestName = "GetRefCusRates_WhenFormulaCanBeLookedUp")]
		[TestCase("15.98256 EURO/1000 t", "0.01598256 * [TNE]", TestName = "GetRefCusRates_WhenFormulaCannotBeLookedUpButConversionIsSupported")]
		[TestCase("0 EURO/1000 kg", "0", TestName = "GetRefCusRates_WhenFormulaCanBeLookedUpButRateIsZero")]
		[TestCase("0", "0", TestName = "GetRefCusRates_WhenFormulaCanBeParsedAsDecimalAndValueIs0")]
		[TestCase("1234", "12.34 * VFD", TestName = "GetRefCusRates_WhenFormulaCanBeParsedAsDecimalAndPercentageValueIsLessThanOrEqual100")]
		[TestCase("12345", "12345", TestName = "GetRefCusRates_WhenFormulaCanBeParsedAsDecimalAndPercentageValueIsGreaterThan100")]
		[TestCase("0.07 EURO/kg", "0.07 * [KGM]", TestName = "GetRefCusRates_WhenFormulaContainsKg")]
		[TestCase("0.00331 EURO/m3", "0.00331 * [MTQ]", TestName = "GetRefCusRates_WhenFormulaContainsM3")]
		public void GetRefCusRates(string inputFormula, string expectedRateFormula)
		{
			const string ergaOmnes = "ERGA OMNES";
			const string rateCodeDescription = "Contributo Stazione Sperimentale Combustibili";
			const string tariffCode = "2701110000";
			var refCusTariff = new RefCusTariff(tariffCode);

			var rateCodeDataLookupMock = new Mock<IRateCodeDataLookup>();
			rateCodeDataLookupMock.Setup(x => x.Lookup(rateCodeDescription, tariffCode)).Returns(new RateCodeDataLookupResult() { Code = "916", RateType = "LEV", RateTypeDataGrouping = "IT" });

			var tradeGroupLookupMock = new Mock<IDataLookup>();
			tradeGroupLookupMock.Setup(x => x.Lookup(ergaOmnes)).Returns("1011");

			var scrappedRecordMock = new Mock<IScrappedRecord>();
			scrappedRecordMock.Setup(x => x.Measure).Returns(new MeasureData(rateCodeDescription, new[] { ergaOmnes }, inputFormula));

			var scrappedRecordParser = new ScrappedRecordParser(tradeGroupLookupMock.Object, new Mock<IDataLookup>().Object, rateCodeDataLookupMock.Object, DateTime.Today);
			Assert.IsNull(scrappedRecordParser.GetRefCusRates(scrappedRecordMock.Object, string.Empty, refCusTariff, new DefaultRateGenerationStrategy()));
			Assert.AreEqual(1, refCusTariff.CusRates.Count, "Rates Count");
			var rate = refCusTariff.CusRates.Single();
			Assert.AreEqual(expectedRateFormula, rate.RateFormula, "Rate Formula");
		}

		[Test]
		public void GetRefCusRates_WhenFormulaContainsEuroTokenButRateIsNotDecimalParsableValue()
		{
			const string ergaOmnes = "ERGA OMNES";
			const string rateCodeDescription = "Contributo Stazione Sperimentale Combustibili";
			const string tariffCode = "2701110000";
			var refCusTariff = new RefCusTariff(tariffCode);

			var rateCodeDataLookupMock = new Mock<IRateCodeDataLookup>();
			rateCodeDataLookupMock.Setup(x => x.Lookup(rateCodeDescription, tariffCode)).Returns(new RateCodeDataLookupResult() { Code = "916", RateType = "LEV", RateTypeDataGrouping = "IT" });

			var tradeGroupLookupMock = new Mock<IDataLookup>();
			tradeGroupLookupMock.Setup(x => x.Lookup(ergaOmnes)).Returns("1011");

			var scrappedRecordMock = new Mock<IScrappedRecord>();
			scrappedRecordMock.Setup(x => x.Measure).Returns(new MeasureData(rateCodeDescription, new[] { ergaOmnes }, "0X EURO/1000 kg"));

			var scrappedRecordParser = new ScrappedRecordParser(tradeGroupLookupMock.Object, new Mock<IDataLookup>().Object, rateCodeDataLookupMock.Object, DateTime.Today);
			var parsingResult = scrappedRecordParser.GetRefCusRates(scrappedRecordMock.Object, string.Empty, refCusTariff, new DefaultRateGenerationStrategy());
			Assert.NotNull(parsingResult, "Parsing Error should be returned");
			Assert.AreEqual("Unable to map Rate Formula", parsingResult.ErrorMessage);
			Assert.AreEqual(0, refCusTariff.CusRates.Count, "Rates Count");
		}

		[TestCase("0.07 EURO/kg", "0.07 * [KGMO]", TestName = "GetRefCusRates_WhenFormulaContainsKgAndRateCodeIs931")]
		[TestCase("1234", "12.34 * VFD", TestName = "GetRefCusRates_WhenFormulaDoesNotContainKgAndRateCodeIs931")]
		public void GetRefCusRates_WhenRateCodeIs931(string inputFormula, string expectedRateFormula)
		{
			const string ergaOmnes = "ERGA OMNES";
			const string rateCodeDescription = "Contributo obblicatorio consorzio oli usati";
			const string tariffCode = "2701110000";
			var refCusTariff = new RefCusTariff(tariffCode);

			var rateCodeDataLookupMock = new Mock<IRateCodeDataLookup>();
			rateCodeDataLookupMock.Setup(x => x.Lookup(rateCodeDescription, tariffCode)).Returns(new RateCodeDataLookupResult() { Code = "931", RateType = "LEV", RateTypeDataGrouping = "IT" });

			var tradeGroupLookupMock = new Mock<IDataLookup>();
			tradeGroupLookupMock.Setup(x => x.Lookup(ergaOmnes)).Returns("1011");

			var scrappedRecordMock = new Mock<IScrappedRecord>();
			scrappedRecordMock.Setup(x => x.Measure).Returns(new MeasureData(rateCodeDescription, new[] { ergaOmnes }, inputFormula));

			var scrappedRecordParser = new ScrappedRecordParser(tradeGroupLookupMock.Object, new Mock<IDataLookup>().Object, rateCodeDataLookupMock.Object, DateTime.Today);
			Assert.IsNull(scrappedRecordParser.GetRefCusRates(scrappedRecordMock.Object, string.Empty, refCusTariff, new DefaultRateGenerationStrategy()));
			Assert.AreEqual(1, refCusTariff.CusRates.Count, "Rates Count");
			var rate = refCusTariff.CusRates.Single();
			Assert.AreEqual(expectedRateFormula, rate.RateFormula, "Rate Formula");
		}

		[TestCaseSource(nameof(GetRefCusVatApplicabilitiesTestCases))]
		public void GetRefCusVatApplicabilitiesCreateVatApplicability(IEnumerable<RequirementData> requirements, string formula, int expectedCount, IEnumerable<string> expectedAdditionalCodes)
		{
			var scrappedRecord = new Mock<IScrappedRecord>();
			var mockTaxOrFeeCodeLookup = new Mock<IDataLookup>();
			mockTaxOrFeeCodeLookup.Setup(x => x.Lookup(It.IsAny<string>())).Returns("ORD");
			var parser = new ScrappedRecordParser(new Mock<IDataLookup>().Object, mockTaxOrFeeCodeLookup.Object,
				new Mock<IRateCodeDataLookup>().Object, DateTime.Today);

			scrappedRecord.Setup(x => x.Measure).Returns(new MeasureData("AAAA", new[] { MeasuresConstant.VatTradeGroup }, formula));
			scrappedRecord.Setup(x => x.Requirement).Returns(requirements);

			var tariff = new RefCusTariff("0201");

			Assert.IsNull(parser.GetRefCusVatApplicabilities(scrappedRecord.Object, tariff));
			Assert.AreEqual(expectedCount, tariff.VatApplicabilities.Count);
			foreach (var expectedAdditionalCode in expectedAdditionalCodes)
			{
				Assert.IsTrue(tariff.VatApplicabilities.Select(x => x.AdditionalCode).Contains(expectedAdditionalCode));
			}
		}

		[Test]
		public void GetRefCusConditionsTest()
		{
			var scrappedRecord = new Mock<IScrappedRecord>();
			var mockTaxOrFeeCodeLookup = new Mock<IDataLookup>();
			mockTaxOrFeeCodeLookup.Setup(x => x.Lookup(It.IsAny<string>())).Returns("ORD");
			var parser = new ScrappedRecordParser(new Mock<IDataLookup>().Object, mockTaxOrFeeCodeLookup.Object,
				new Mock<IRateCodeDataLookup>().Object, DateTime.Today);

			var measure = new MeasureData(ConditionConstants.conditionTypeDictionary.First().Key, new[] { MeasuresConstant.VatTradeGroup }, "CertificatoEscluso:AD, CH");
			var requirement = new RequirementData("One:Two", " Requirement Description ");

			scrappedRecord.Setup(x => x.Requirement).Returns(new List<RequirementData>() { requirement });
			scrappedRecord.Setup(x => x.Measure).Returns(measure);

			var tariff = new RefCusTariff("0201");
			parser.GetRefCusConditions(scrappedRecord.Object, tariff, new List<CertificateData>() { new CertificateData("C123", "true") });
			Assert.IsTrue(tariff.CusConditions.Count == 1);
			Assert.AreEqual("Altri obblighi", tariff.CusConditions.First().Comment);
			Assert.IsTrue(tariff.CusConditions.First().Applicability != null);
			Assert.IsTrue(tariff.CusConditions.First().Applicability.ExcludedTradeGroups.Count == 2);
			Assert.AreEqual("AD", tariff.CusConditions.First().Applicability.ExcludedTradeGroups[0].TradeGroup);
			Assert.AreEqual("CH", tariff.CusConditions.First().Applicability.ExcludedTradeGroups[1].TradeGroup);
			Assert.IsTrue(tariff.CusConditions.First().ConditionValue != null);
			Assert.AreEqual("C123", tariff.CusConditions.First().ConditionValue[0].Value);
			Assert.AreEqual("SUP", tariff.CusConditions.First().ConditionValue[0].ValueType);
		}

		static IEnumerable GetRefCusVatApplicabilitiesTestCases
		{
			get
			{
				yield return new TestCaseData(null, "0", 1, new[] { "" })
				{
					TestName = "GetRefCusVatApplicabilitiesCreateVatApplicability_IfNoRequirementPresent"
				};

				yield return new TestCaseData(new List<RequirementData> { null }, "0", 1, new[] { "" })
				{
					TestName = "GetRefCusVatApplicabilitiesCreateVatApplicability_IfNullRequirementPresent"
				};

				yield return new TestCaseData(new List<RequirementData> { new RequirementData("regolamento", "1/012345 9876") }, "0", 1, new[] { "" })
				{
					TestName = "GetRefCusVatApplicabilitiesCreateVatApplicability_IfNonEmptyRequirementPresent"
				};

				yield return new TestCaseData(new List<RequirementData> { new RequirementData("regolamento", "1/012345 9876"), new RequirementData("Cadd", "Q1234") }, "0", 1, new[] { "Q1234" })
				{
					TestName = "GetRefCusVatApplicabilitiesCreateVatApplicability_IfAdditionalCodeRequirementPresent"
				};

				yield return new TestCaseData(new List<RequirementData> { new RequirementData("Cadd", "Q1234") }, "0", 1, new[] { "Q1234" })
				{
					TestName = "GetRefCusVatApplicabilitiesCreateVatApplicability_IfOnlyAdditionalCodeRequirementPresent"
				};

				yield return new TestCaseData(new RequirementData[0], null, 0, new string[0])
				{
					TestName = "GetRefCusVatApplicabilitiesDoesNotCreateVatApplicability_IfFormulaIsNull"
				};

				yield return new TestCaseData(new RequirementData[0], "", 0, new string[0])
				{
					TestName = "GetRefCusVatApplicabilitiesDoesNotCreateVatApplicability_IfFormulaIsEmpty"
				};

				yield return new TestCaseData(new RequirementData[0], " ", 0, new string[0])
				{
					TestName = "GetRefCusVatApplicabilitiesDoesNotCreateVatApplicability_IfFormulaIsWhitespace"
				};
			}
		}
	}
}
