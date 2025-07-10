namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondVehicleCtrlValidationSailingSynchronisationTest : LinkedSailingBillsImportedTest
	{
		public void TestVehicleNotLinkToSailing()
		{
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "SCAC";
			bill.B0_MasterBillNumber = "BBB";
			var container = bill.MovementDetail.Containers.AddNew();
			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			var vehicle = container.Vehicles.AddNew();
			vehicle.BV_VIN = "VIN1";
			AssertHasWarning(vehicle.BV_VINInfo, ValidationConstants.SailingSynchronisation.VehicleMightBeIncorectlyAdded.ToString());
			vehicle.BV_VIN = "VIN";
			AssertNoWarning(vehicle.BV_VINInfo, ValidationConstants.SailingSynchronisation.VehicleMightBeIncorectlyAdded.ToString());
		}
	}
}
