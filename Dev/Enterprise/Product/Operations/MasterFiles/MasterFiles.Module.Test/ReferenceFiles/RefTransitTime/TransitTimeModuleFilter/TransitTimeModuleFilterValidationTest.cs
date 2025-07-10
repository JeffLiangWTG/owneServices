using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class TransitTimeModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckComparisonOperator()
		{
			var filter = GetNewFilter();

			filter.ComparisonOperator = ZString.Empty;
			AssertHasError(filter.ComparisonOperatorInfo, "Please enter a value.");

			filter.ComparisonOperator = "XXX";
			AssertHasError(filter.ComparisonOperatorInfo, "Enter a valid selection.");

			filter.ComparisonOperator = "Greater than";
			AssertNoErrors(filter.ComparisonOperatorInfo);
		}

		public void TestCheckTransitDays()
		{
			var filter = GetNewFilter();

			filter.TransitDays = -5;
			AssertHasError(filter.TransitDaysInfo, "value cannot be negative.");

			filter.TransitDays = 5;
			AssertNoErrors(filter.TransitDaysInfo);
		}

		public void TestCheckTransitHours()
		{
			var filter = GetNewFilter();

			filter.TransitHours = -5;
			AssertHasError(filter.TransitHoursInfo, "value cannot be negative.");

			filter.TransitHours = 5;
			AssertNoErrors(filter.TransitHoursInfo);
		}

		TransitTimeModuleFilter GetNewFilter()
		{
			return new TransitTimeModuleFilter("desc", RefTransitTimeSchema.RTT_TransitHours);
		}
	}
}
