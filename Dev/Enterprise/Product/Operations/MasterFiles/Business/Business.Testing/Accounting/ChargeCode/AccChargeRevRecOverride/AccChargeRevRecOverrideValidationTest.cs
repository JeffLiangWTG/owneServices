using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeRevRecOverrideValidationTest : BusinessObjectValidationTestCase
	{
		#region CheckAE_AC

		public void TestNoErrorWhenAccChargeCodeIsInactive()
		{
			var inactiveAccChargeCode = Factory.New<AccChargeCode>();
			inactiveAccChargeCode.AC_IsActive = false;

			var revenueRecOverride = inactiveAccChargeCode.RevenueRecOverrides.AddNew();

			AssertNoErrors(revenueRecOverride.AE_ACInfo);
		}

		#endregion
	}
}
