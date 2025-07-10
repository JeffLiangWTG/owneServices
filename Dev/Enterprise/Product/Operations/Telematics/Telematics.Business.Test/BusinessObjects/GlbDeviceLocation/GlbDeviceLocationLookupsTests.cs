using CargoWise.EntityFramework.Testing;

namespace Enterprise.Telematics.Business.Test
{
	class GlbDeviceLocationLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestSpeedLimitStateListIsNotNull()
		{
			// Arrange
			var location = Factory.New<GlbDeviceLocation>();

			// Act
			var result = location.Lookups.SpeedLimitStateList;

			// Assert
			AssertNotNull(result);
			AssertEquals(3, result.Count);
		}
	}
}
