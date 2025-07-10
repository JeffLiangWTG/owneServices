using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Test
{
	static class RefCusRateAssertionHelper
	{
		public static void AssertRefCusRate(
			RefCusRate actualRate,
			string expectedRateCode,
			string expectedRateType,
			string expectedRateFormula,
			string expectedPreference,
			string expectedPreferenceDataGrouping,
			string expectedStartDate,
			string expectedApplicabilityTradeGroup,
			string expectedApplicabilityAdditionalCode,
			string expectedApplicabilityStartDate,
			string[] expectedMeasurmentUnits)
		{
			Assert.AreEqual(expectedRateCode, actualRate.RateCode);
			Assert.AreEqual(expectedRateType, actualRate.RateType);
			Assert.AreEqual(expectedRateFormula, actualRate.RateFormula);
			Assert.AreEqual(expectedPreference, actualRate.Preference);
			Assert.AreEqual(expectedPreferenceDataGrouping, actualRate.PreferenceDataGrouping);
			Assert.AreEqual(expectedStartDate, actualRate.StartDate);

			var actualApplicability = actualRate.Applicability;
			Assert.IsNotNull(actualApplicability);
			Assert.AreEqual(expectedApplicabilityTradeGroup, actualApplicability.TradeGroup);
			Assert.AreEqual(expectedApplicabilityAdditionalCode, actualApplicability.AdditionalCode);
			Assert.AreEqual(expectedApplicabilityStartDate, actualApplicability.StartDate);
			CollectionAssert.AreEqual(expectedMeasurmentUnits, actualRate.RateUOMs?.Select(x => x.UOM));
		}
	}
}
