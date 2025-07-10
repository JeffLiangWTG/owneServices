using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondVehicleCtrlValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBV_VIN()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			var vehicle = container.Vehicles.AddNew();
			vehicle.Validation.ValidateBV_VIN();
			AssertHasMessageErrorContaining(vehicle.BV_VINInfo, MandatoryValidation.YouHaveNotEntered);
			vehicle.BV_VIN = "123";
			AssertNoMessageErrorContaining(vehicle.BV_VINInfo, MandatoryValidation.YouHaveNotEntered);
			var vehicle2 = container.Vehicles.AddNew();
			vehicle2.BV_VIN = "123";
			AssertHasMessageErrorContaining(vehicle2.BV_VINInfo, CusInBondVehicleCtrlValidation.DuplicateVINIsNotAllowed);
			vehicle2.BV_VIN = "124";
			AssertNoMessageErrorContaining(vehicle2.BV_VINInfo, CusInBondVehicleCtrlValidation.DuplicateVINIsNotAllowed);
		}
	}
}
