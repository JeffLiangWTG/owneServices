using System;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	[TestFixture]
	class DefaultRateGenerationStrategyFixture
	{
		[Test]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => rateGenerationStrategy.GenerateRates(targetTariff: null, refCusRateMock.Object));
			Assert.Throws<ArgumentNullException>(() => rateGenerationStrategy.GenerateRates(new RefCusTariff("0123456789"), rate: null));
		}

		[Test]
		public void GenerateRate()
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
			refCusRateMock = new Mock<IRate>();
			rateGenerationStrategy = new DefaultRateGenerationStrategy();
		}

		RefCusTariff tariff;
		Mock<IRate> refCusRateMock;
		IRateGenerationStrategy rateGenerationStrategy;
	}
}
