namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbRoutePlannerAssignConsignmentsInfoTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestConstruction

		public void TestConstruction()
		{
			var carrier = Helper.CreateOrganisation("123");
			var entity1 = new DtbRoutePlannerAssignConsignmentsInfo(carrier);
			AssertEquals(carrier, entity1.Carrier);
			AssertEquals(RunSheetView.Carriers, entity1.AssigningType);
			AssertNull(entity1.Driver);
			AssertNull(entity1.Vehicle);

			var vehicle = Helper.CreateVehicle(Helper.CreateDriver("BOB", "BOB"), "123");
			var entity2 = new DtbRoutePlannerAssignConsignmentsInfo(vehicle);
			AssertEquals(vehicle, entity2.Vehicle);
			AssertEquals(RunSheetView.Vehicles, entity2.AssigningType);
			AssertNull(entity2.Driver);
			AssertNull(entity2.Carrier);

			var driver = Helper.CreateDriver("BOB", "BOB");
			var entity3 = new DtbRoutePlannerAssignConsignmentsInfo(driver);
			AssertEquals(driver, entity3.Driver);
			AssertEquals(RunSheetView.Drivers, entity3.AssigningType);
			AssertNull(entity3.Vehicle);
			AssertNull(entity3.Carrier);
		}

		#endregion
	}
}
