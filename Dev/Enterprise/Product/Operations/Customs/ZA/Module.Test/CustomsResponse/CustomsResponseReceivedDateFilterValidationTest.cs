using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class CustomsResponseReceivedDateFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDateRange()
		{
			ModuleDateFilter filter = new CustomsResponseReceivedDateFilter("Time Received");
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			AssertHasError(filter.Property1Info, "From date must not be empty. Please enter a value.");
			AssertNoError(filter.Property1Info, "Date range should be no longer than one week.");
			filter.Property2 = ZDateTime.Empty;
			AssertHasError(filter.Property2Info, "To date must not be empty. Please enter a value.");
			AssertNoError(filter.Property2Info, "Date range should be no longer than one week.");
			filter.Property1 = new ZDateTime(2016, 12, 20);
			filter.Property2 = new ZDateTime(2016, 12, 27);
			filter.Validation.ValidateProperty1();
			AssertNoError(filter.Property1Info, "From date must not be empty. Please enter a value.");
			AssertHasError(filter.Property1Info, "Date range should be no longer than one week.");
			filter.Validation.ValidateProperty2();
			AssertNoError(filter.Property2Info, "To date must not be empty. Please enter a value.");
			AssertHasError(filter.Property2Info, "Date range should be no longer than one week.");
			filter.Property2 = new ZDateTime(2016, 12, 26);
			filter.Validation.ValidateProperty1();
			AssertNoError(filter.Property1Info, "From date must not be empty. Please enter a value.");
			AssertNoError(filter.Property1Info, "Date range should be no longer than one week.");
			filter.Validation.ValidateProperty2();
			AssertNoError(filter.Property2Info, "To date must not be empty. Please enter a value.");
			AssertNoError(filter.Property2Info, "Date range should be no longer than one week.");
		}
	}
}
