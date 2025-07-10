using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgFreeWaitingTimeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOY_DropMode()
		{
			var orgFreeWaitingTime = Factory.NewWithValidTestData<OrgFreeWaitingTime>();
			AssertNoErrors(orgFreeWaitingTime.OY_DropModeInfo);

			orgFreeWaitingTime.OY_DropMode = "";
			AssertHasError(orgFreeWaitingTime.OY_DropModeInfo, "Drop Mode cannot be empty. Use 'ANY' for a catch all fallback.");

			orgFreeWaitingTime.OY_DropMode = "ZZZ";
			AssertHasError(orgFreeWaitingTime.OY_DropModeInfo, "Please use a valid Drop Mode from the list.");
		}

		public void TestCheckOY_RC_ContainerType()
		{
			var orgFreeWaitingTime = Factory.NewWithValidTestData<OrgFreeWaitingTime>();
			AssertNoErrors(orgFreeWaitingTime.OY_RC_ContainerTypeInfo);

			orgFreeWaitingTime.OY_RC_ContainerType = ZGuid.Missing;
			AssertHasError(orgFreeWaitingTime.OY_RC_ContainerTypeInfo, "Please enter a valid Container Type.");

			orgFreeWaitingTime.OY_RC_ContainerType = ZGuid.Empty;
			AssertNoErrors(orgFreeWaitingTime.OY_RC_ContainerTypeInfo);
		}
	}
}
