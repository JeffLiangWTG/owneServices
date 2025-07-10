using System;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class NoteA148RateGenerationStrategyFixture
	{
		[Test]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new NoteA148RateGenerationStrategy(preferenceDataLookup: null));
			Assert.Throws<ArgumentNullException>(() => rateGenerationStrategy.GenerateRates(targetTariff: null, refCusRateMock.Object));
			Assert.Throws<ArgumentNullException>(() => rateGenerationStrategy.GenerateRates(new RefCusTariff("0123456789"), rate: null));
		}

		[Test]
		public void GenerateMultipliedLeviesRates()
		{
			refCusRateMock.Setup(x => x.RateCode)
				.Returns("912");

			refCusRateMock.Setup(x => x.RateType)
				.Returns("LEV");

			refCusRateMock.Setup(x => x.RateFormula)
				.Returns("0.020889 * [TNE]");

			refCusRateMock.Setup(x => x.StartDate)
				.Returns(new DateTime(2024, 01, 18));

			refCusRateMock.Setup(x => x.Applicability.TradeGroup)
				.Returns("1011");

			refCusRateMock.Setup(x => x.Applicability.AdditionalCode)
				.Returns("R042");

			refCusRateMock.Setup(x => x.Applicability.StartDate)
				.Returns(new DateTime(2024, 01, 18));

			refCusRateMock.Setup(x => x.MeasurementUnits)
				.Returns(new[] { "TNE" }.ToHashSet());

			Assert.AreEqual(0, tariff.CusRates.Count, "[PRE-CONDITION] CusRates Count");

			rateGenerationStrategy.GenerateRates(tariff, refCusRateMock.Object);

			Assert.AreEqual(2, tariff.CusRates.Count, "[POST-CONDITION] CusRates Count");

			RefCusRateAssertionHelper.AssertRefCusRate(
				tariff.CusRates[0],
				"912",
				"LEV",
				"0.020889 * [TNE]",
				"100",
				"EUN",
				"2024-01-18T00:00:00",
				"1011",
				"R042",
				"2024-01-18T00:00:00",
				new[] { "TNE" });
			RefCusRateAssertionHelper.AssertRefCusRate(
				tariff.CusRates[1],
				"912",
				"LEV",
				"0.020889 * [TNE]",
				"200",
				"EUN",
				"2024-01-18T00:00:00",
				"1011",
				"R042",
				"2024-01-18T00:00:00",
				new[] { "TNE" });
		}

		[Test]
		public void GenerateSingleNonLeviesRate()
		{
			refCusRateMock.Setup(x => x.RateCode)
				.Returns("125");

			refCusRateMock.Setup(x => x.RateType)
				.Returns("MSC");

			refCusRateMock.Setup(x => x.RateFormula)
				.Returns("30.99 * [TNE]");

			refCusRateMock.Setup(x => x.StartDate)
				.Returns(new DateTime(2024, 01, 18));

			refCusRateMock.Setup(x => x.Applicability.TradeGroup)
				.Returns("1011");

			refCusRateMock.Setup(x => x.Applicability.AdditionalCode)
				.Returns("R042");

			refCusRateMock.Setup(x => x.Applicability.StartDate)
				.Returns(new DateTime(2024, 01, 18));

			refCusRateMock.Setup(x => x.MeasurementUnits)
				.Returns(new[] { "TNE" }.ToHashSet());

			Assert.AreEqual(0, tariff.CusRates.Count, "[PRE-CONDITION] CusRates Count");

			rateGenerationStrategy.GenerateRates(tariff, refCusRateMock.Object);

			Assert.AreEqual(1, tariff.CusRates.Count, "[POST-CONDITION] CusRates Count");
			RefCusRateAssertionHelper.AssertRefCusRate(
				tariff.CusRates[0],
				"125",
				"MSC",
				"30.99 * [TNE]",
				null,
				null,
				"2024-01-18T00:00:00",
				"1011",
				"R042",
				"2024-01-18T00:00:00",
				new[] { "TNE" });
		}

		[SetUp]
		protected void SetUp()
		{
			tariff = new RefCusTariff("0123456789");

			var preference100 = new PreferenceData() { Code = "100", DataGrouping = "EUN" };
			var preference200 = new PreferenceData() { Code = "200", DataGrouping = "EUN" };

			var preferenceDataLookupMock = new Mock<IPreferenceDataLookup>();
			preferenceDataLookupMock.Setup(x => x.Preferences)
				.Returns(new[] { preference100, preference200 });

			refCusRateMock = new Mock<IRate>();
			rateGenerationStrategy = new NoteA148RateGenerationStrategy(preferenceDataLookupMock.Object);
		}

		RefCusTariff tariff;
		Mock<IRate> refCusRateMock;
		IRateGenerationStrategy rateGenerationStrategy;
	}
}
