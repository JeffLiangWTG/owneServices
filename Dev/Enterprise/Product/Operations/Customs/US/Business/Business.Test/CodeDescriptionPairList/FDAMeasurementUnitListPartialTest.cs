using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FDAMeasurementUnitListTest : TestCase
	{
		public void TestConvert()
		{
			AssertEquals(12.15m, FDAMeasurementUnitList.ConvertToInchesWithOneSixteenth(12.15m, FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals));

			AssertEquals(4.02m, FDAMeasurementUnitList.ConvertToInchesWithOneSixteenth(10.50m, FDAMeasurementUnitList.Codes.Centimeters).Truncate(2));

			AssertEquals(0m, FDAMeasurementUnitList.ConvertToInchesWithOneSixteenth(10.50m, ""));

			AssertEquals(0m, FDAMeasurementUnitList.ConvertToInchesWithOneSixteenth(0m, FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals));

			AssertEquals(3.08m, FDAMeasurementUnitList.ConvertToInchesWithOneSixteenth(3.5m, FDAMeasurementUnitList.Codes.InchesWithOneTenthDecimals).Truncate(2));
		}
	}
}
