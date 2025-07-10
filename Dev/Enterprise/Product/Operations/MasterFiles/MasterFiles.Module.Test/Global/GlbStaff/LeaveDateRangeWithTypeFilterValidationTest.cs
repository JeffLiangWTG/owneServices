using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class LeaveDateRangeWithTypeFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateWorkHolidayType()
		{
			var filter = new LeaveDateRangeWithTypeFilter("moo") { WorkHolidayType = "222" };
			AssertListValidationInvalidCodeError(filter.WorkHolidayTypeInfo, true);

			filter.WorkHolidayType = "ANN";
			AssertListValidationInvalidCodeError(filter.WorkHolidayTypeInfo, false);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertHasError(filter.WorkHolidayTypeInfo, "Leave type can not be used together with 'Has No Date'.");

			filter.WorkHolidayType = "";
			AssertNoErrors(filter.WorkHolidayTypeInfo);

			filter.WorkHolidayType = "ANN";
			AssertHasError(filter.WorkHolidayTypeInfo, "Leave type can not be used together with 'Has No Date'.");
		}
	}
}
