using CargoWise.EntityFramework.Testing;

namespace Enterprise.Telematics.Business.Test
{
	class TelSubEquipmentLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestTelSubEquipmentTypeListIsNotNull()
		{
			// Arrange
			var location = Factory.New<TelSubEquipment>();

			// Act
			var result = location.Lookups.TelSubEquipmentTypeList;

			// Assert
			AssertNotNull(result);
			AssertEquals(3, result.Count);
		}
	}
}
