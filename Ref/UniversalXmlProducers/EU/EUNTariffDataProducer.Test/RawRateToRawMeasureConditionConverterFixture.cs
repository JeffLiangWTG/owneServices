using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class RawRateToRawMeasureConditionConverterFixture
	{
		[Test]
		public void Conversion_IsImportFalse()
		{
			var rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410",
				"Cond:  B cert: C-057 (29):; B cert: C-079 (29):; B cert: C-082 (29):; B cert: Y-120 (29):; B cert: Y-950 (29):; B (09):; Y cert: Y-053 (29):; Y cert: Y-054 (29):; Y (09):; YA cert: Y-123 (29):; YA cert: Y-124 (29):; YA cert: Y-976 (29):; YA (09):",
				"", false);

			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);
			Assert.That(result.All(x => !x.IsImport));
		}

		[Test]
		public void Conversion_IsImportTrue()
		{
			var rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410",
				"Cond:  B cert: C-057 (29):; B cert: C-079 (29):; B cert: C-082 (29):; B cert: Y-120 (29):; B cert: Y-950 (29):; B (09):; Y cert: Y-053 (29):; Y cert: Y-054 (29):; Y (09):; YA cert: Y-123 (29):; YA cert: Y-124 (29):; YA cert: Y-976 (29):; YA (09):",
				"", true);

			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);
			Assert.That(result.All(x => x.IsImport));
		}

		[Test]
		public void Conversion_ConditionCodeDoubleDigit()
		{
			var rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410",
				"Cond:  B cert: C-057 (29):; B cert: C-079 (29):; B cert: C-082 (29):; B cert: Y-120 (29):; B cert: Y-950 (29):; B (09):; Y cert: Y-053 (29):; Y cert: Y-054 (29):; Y (09):; YA cert: Y-123 (29):; YA cert: Y-124 (29):; YA cert: Y-976 (29):; YA (09):",
				"");

			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);
			Assert.That(result.Any(x => x.MeasureConditionCode == "YA"), Is.True);
		}

		[Test]
		public void Conversion_InformationalValueType()
		{
			var rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410",
				"Cond:  A cert: D-008 (01):115.600 EUR TNE I ; A (01):172.200 EUR TNE I", "");

			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();
			var result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);
			Assert.That(result.Count(), Is.EqualTo(2));

			var informationalCondition = result.ElementAt(1);
			Assert.That(informationalCondition.ConditionAmount, Is.EqualTo("Apply the mentioned duty of 172.200 * [TNEI]"));

			rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410",
				"Cond:  A cert: D-008 (01):115.600 EUR TNE I ; A (01):", "");
			result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);
			Assert.That(result.Count(), Is.EqualTo(2));
			informationalCondition = result.ElementAt(1);

			Assert.That(informationalCondition.ConditionAmount, Is.EqualTo("Not presented"));
		}

		[Test]
		public void ConversionTest_CertificateOnly()
		{
			var rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410",
				"Cond:  B cert: C-640 (29):; B cert: Y-072 (29):; B cert: Y-073 (29):; B cert: Y-077 (29):; B cert: Y-078 (29):; B cert: Y-079 (29):; B (09):",
				"");
			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);

			var resultList = result.ToList();
			Assert.AreEqual(6, resultList.Count);

			var firstRecord = resultList.First();
			Assert.AreEqual("0102030405", firstRecord.TariffHeader);
			Assert.AreEqual("", firstRecord.AdditionalCode);
			Assert.AreEqual("", firstRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, firstRecord.StartDate);
			Assert.AreEqual(DateTime.Today, firstRecord.EndDate);
			Assert.AreEqual("1011", firstRecord.TradeGroup);
			Assert.AreEqual("410", firstRecord.MeasureTypeId);
			Assert.AreEqual("B", firstRecord.MeasureConditionCode);
			Assert.AreEqual("C640", firstRecord.CertificateTypeCode);
			Assert.AreEqual("", firstRecord.ConditionAmount);
			Assert.AreEqual("", firstRecord.MonetaryUnitCode);
			Assert.AreEqual("", firstRecord.MeasureUnit);
			Assert.AreEqual("29", firstRecord.MeasureAction);

			var lastRecord = resultList.Last();
			Assert.AreEqual("0102030405", lastRecord.TariffHeader);
			Assert.AreEqual("", lastRecord.AdditionalCode);
			Assert.AreEqual("", lastRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, lastRecord.StartDate);
			Assert.AreEqual(DateTime.Today, lastRecord.EndDate);
			Assert.AreEqual("1011", lastRecord.TradeGroup);
			Assert.AreEqual("410", lastRecord.MeasureTypeId);
			Assert.AreEqual("B", lastRecord.MeasureConditionCode);
			Assert.AreEqual("Y079", lastRecord.CertificateTypeCode);
			Assert.AreEqual("", lastRecord.ConditionAmount);
			Assert.AreEqual("", lastRecord.MonetaryUnitCode);
			Assert.AreEqual("", lastRecord.MeasureUnit);
			Assert.AreEqual("29", lastRecord.MeasureAction);
		}

		[Test]
		public void ConversionTest_FormulaOnly()
		{
			var rate = new RawRateRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "", "", "", "1011", "410", "Cond:  E 20.000/KGM(29):; E cert: Y-923 (29):; E (09):; I cert: C-672 (29):; I 20.000 EUR/KGM(29):; I (09):", "");
			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(rate);
			Assert.IsNotNull(result);

			var resultList = result.ToList();
			Assert.AreEqual(4, resultList.Count);

			var firstRecord = resultList.First();
			Assert.AreEqual("0102030405", firstRecord.TariffHeader);
			Assert.AreEqual("", firstRecord.AdditionalCode);
			Assert.AreEqual("", firstRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, firstRecord.StartDate);
			Assert.AreEqual(DateTime.Today, firstRecord.EndDate);
			Assert.AreEqual("1011", firstRecord.TradeGroup);
			Assert.AreEqual("410", firstRecord.MeasureTypeId);

			Assert.AreEqual("E", firstRecord.MeasureConditionCode);
			Assert.AreEqual("", firstRecord.CertificateTypeCode);
			Assert.AreEqual("20.000", firstRecord.ConditionAmount);
			Assert.AreEqual("", firstRecord.MonetaryUnitCode);
			Assert.AreEqual("KGM", firstRecord.MeasureUnit);
			Assert.AreEqual("29", firstRecord.MeasureAction);

			var lastRecord = resultList.Last();
			Assert.AreEqual("0102030405", lastRecord.TariffHeader);
			Assert.AreEqual("", lastRecord.AdditionalCode);
			Assert.AreEqual("", lastRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, lastRecord.StartDate);
			Assert.AreEqual(DateTime.Today, lastRecord.EndDate);
			Assert.AreEqual("1011", lastRecord.TradeGroup);
			Assert.AreEqual("410", lastRecord.MeasureTypeId);

			Assert.AreEqual("I", lastRecord.MeasureConditionCode);
			Assert.AreEqual("", lastRecord.CertificateTypeCode);
			Assert.AreEqual("20.000", lastRecord.ConditionAmount);
			Assert.AreEqual("EUR", lastRecord.MonetaryUnitCode);
			Assert.AreEqual("KGM", lastRecord.MeasureUnit);
			Assert.AreEqual("29", lastRecord.MeasureAction);
		}

		[Test]
		public void ConversionTest_STypeMeasureCondition()
		{
			var measureCondition = new RawMeasureConditionRecord("0102030405", "", "", DateTime.Today, DateTime.Today, "1011", "123", "S", "", "28", "EUR", "NAR", "27", "");
			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(new[] { measureCondition });
			Assert.IsNotNull(result);

			var firstRecord = result.First();
			Assert.AreEqual("0102030405", firstRecord.TariffHeader);
			Assert.AreEqual("", firstRecord.AdditionalCode);
			Assert.AreEqual("", firstRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, firstRecord.StartDate);
			Assert.AreEqual(DateTime.Today, firstRecord.EndDate);
			Assert.AreEqual("28 EUR NAR", firstRecord.Rate);
			Assert.AreEqual("SEC", firstRecord.RateCode);
		}

		[Test]
		public void ConversionTest_NoOtherCondition()
		{
			var rate1 = new RawRateRecord("1516209821", "A999", "", DateTime.Today, DateTime.Today, "", "Definitive countervailing duty", "", "1011", "552", "237.000 EUR TNE I", "");

			var rate2 = new RawRateRecord("4302191500", "4999", "", DateTime.Today, DateTime.Today, "", "Restriction on entry into free circulation", "", "1011", "475", "", "");
			var rateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			var result = rateToRawMeasureConditionConverter.Convert(rate1);
			Assert.IsNotNull(result);

			var resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count);

			var firstRecord = resultList.First();
			Assert.AreEqual("1516209821", firstRecord.TariffHeader);
			Assert.AreEqual("A999", firstRecord.AdditionalCode);
			Assert.AreEqual("", firstRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, firstRecord.StartDate);
			Assert.AreEqual(DateTime.Today, firstRecord.EndDate);
			Assert.AreEqual("1011", firstRecord.TradeGroup);
			Assert.AreEqual("552", firstRecord.MeasureTypeId);

			Assert.AreEqual(ApplicationConfig.NotApplicableForNoOtherCondition, firstRecord.MeasureConditionCode);
			Assert.AreEqual("", firstRecord.CertificateTypeCode);
			Assert.AreEqual("", firstRecord.ConditionAmount);
			Assert.AreEqual("", firstRecord.MonetaryUnitCode);
			Assert.AreEqual("", firstRecord.MeasureUnit);
			Assert.AreEqual(ApplicationConfig.NotApplicableForNoOtherCondition, firstRecord.MeasureAction);
			Assert.AreEqual("Definitive countervailing duty", firstRecord.Comment);

			result = rateToRawMeasureConditionConverter.Convert(rate2);
			Assert.IsNotNull(result);

			resultList = result.ToList();
			Assert.AreEqual(1, resultList.Count);

			firstRecord = resultList.First();
			Assert.AreEqual("4302191500", firstRecord.TariffHeader);
			Assert.AreEqual("4999", firstRecord.AdditionalCode);
			Assert.AreEqual("", firstRecord.OrderNumber);
			Assert.AreEqual(DateTime.Today, firstRecord.StartDate);
			Assert.AreEqual(DateTime.Today, firstRecord.EndDate);
			Assert.AreEqual("1011", firstRecord.TradeGroup);
			Assert.AreEqual("475", firstRecord.MeasureTypeId);

			Assert.AreEqual(ApplicationConfig.NotApplicableForNoOtherCondition, firstRecord.MeasureConditionCode);
			Assert.AreEqual("", firstRecord.CertificateTypeCode);
			Assert.AreEqual("", firstRecord.ConditionAmount);
			Assert.AreEqual("", firstRecord.MonetaryUnitCode);
			Assert.AreEqual("", firstRecord.MeasureUnit);
			Assert.AreEqual(ApplicationConfig.NotApplicableForNoOtherCondition, firstRecord.MeasureAction);
			Assert.AreEqual("Restriction on entry into free circulation", firstRecord.Comment);
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.ConfigEnvironment();
		}
	}
}
