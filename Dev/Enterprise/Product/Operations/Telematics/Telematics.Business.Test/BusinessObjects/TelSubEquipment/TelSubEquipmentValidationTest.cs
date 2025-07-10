using CargoWise.EntityFramework.Testing;

namespace Enterprise.Telematics.Business.Test
{
	class TelSubEquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSE_Type()
		{
			// Arrange
			var telSubEquipment = Factory.NewWithValidTestData<TelSubEquipment>();

			// Act
			// Assert
			telSubEquipment.TSE_Type = "A";
			AssertNoErrors("State is valid, should not have errors", telSubEquipment.TSE_TypeInfo);
			telSubEquipment.TSE_Type = "B";
			AssertHasErrors("State is NOT valid, should have errors", telSubEquipment.TSE_TypeInfo);
			telSubEquipment.TSE_Type = "O";
			AssertNoErrors("State is valid, should not have errors", telSubEquipment.TSE_TypeInfo);
			telSubEquipment.TSE_Type = "P";
			AssertHasErrors("State is NOT valid, should have errors", telSubEquipment.TSE_TypeInfo);
			telSubEquipment.TSE_Type = "W";
			AssertNoErrors("State is valid, should not have errors", telSubEquipment.TSE_TypeInfo);
			telSubEquipment.TSE_Type = "U";
			AssertHasErrors("State is NOT valid, should have errors", telSubEquipment.TSE_TypeInfo);
		}
	}
}
