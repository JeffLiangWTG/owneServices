using System.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class NumberOfDecimalsHelperTest : TestCaseWithFactory
	{
		public void TestNumberOfDecimalsForNonNumericProperties()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			AssertEquals(-1, NumberOfDecimalsHelper.GetNumberOfDecimals(rateLine, rateLine.ER_WeightInLBsOrKGsInfo, TestNumberOfDecimalsGetter));
		}

		public void TestGetNumberOfDecimals()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			AssertEquals(1, NumberOfDecimalsHelper.GetNumberOfDecimals(rateLine, rateLine.ER_GrossWeightInfo, TestNumberOfDecimalsGetter));
		}

		int TestNumberOfDecimalsGetter(PropertyDescriptor property)
		{
			return 1;
		}
	}
}
