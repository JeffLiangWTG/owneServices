using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class ValidationModesCalculatorTest : TestCaseWithFactory
	{
		public void TestIsThisValidationOn()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.Save();//will have JE_JobReference assigned and it will be marked as invalid

			AssertEquals(ValidationModes.InventoryRecord, header.ValidationModes);
			AssertEquals(true, ValidationModesCalculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InventoryRecord));

			header.ValidationModes = ValidationModes.None;
			AssertEquals(false, ValidationModesCalculator.IsThisValidationOn(header.ValidationModes, ValidationModes.InventoryRecord));

			header.ValidationModes = ValidationModes.PermitToTransfer;
			AssertEquals(true, ValidationModesCalculator.IsThisValidationOn(header.ValidationModes, ValidationModes.PermitToTransfer));
			AssertEquals(false, ValidationModesCalculator.IsThisValidationOn(header.ValidationModes, ValidationModes.UseParentValidateMode));

			header.ValidationModes = ValidationModes.VesselDeparture;
			AssertEquals(true, ValidationModesCalculator.IsThisValidationOn(header.ValidationModes, ValidationModes.VesselDeparture));
			AssertEquals(false, ValidationModesCalculator.IsThisValidationOn(header.ValidationModes, ValidationModes.UseParentValidateMode));
		}
	}
}
