using CargoWise.EntityFramework.Testing;

namespace Enterprise.Telematics.Business.Test
{
	class GlbDeviceLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckV2_SpeedLimitState()
		{
			// Arrange
			var subscription = Factory.NewWithValidTestData<GlbDeviceLocation>();

			// Act
			// Assert
			subscription.V2_SpeedLimitState = "M";
			AssertNoErrors("State is valid, should not have errors", subscription.V2_SpeedLimitStateInfo);
			subscription.V2_SpeedLimitState = "A";
			AssertHasErrors("State is NOT valid, should have errors", subscription.V2_SpeedLimitStateInfo);
			subscription.V2_SpeedLimitState = "S";
			AssertNoErrors("State is valid, should not have errors", subscription.V2_SpeedLimitStateInfo);
			subscription.V2_SpeedLimitState = "B";
			AssertHasErrors("State is NOT valid, should have errors", subscription.V2_SpeedLimitStateInfo);
			subscription.V2_SpeedLimitState = "U";
			AssertNoErrors("State is valid, should not have errors", subscription.V2_SpeedLimitStateInfo);
			subscription.V2_SpeedLimitState = "C";
			AssertHasErrors("State is NOT valid, should have errors", subscription.V2_SpeedLimitStateInfo);
		}
	}
}
