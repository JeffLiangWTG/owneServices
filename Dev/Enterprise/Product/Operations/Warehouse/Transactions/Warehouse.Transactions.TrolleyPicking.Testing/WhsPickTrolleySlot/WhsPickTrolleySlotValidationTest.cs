namespace Enterprise.Warehouse.Transactions.TrolleyPicking.Testing
{
	using Business.Testing;

	internal class WhsPickTrolleySlotValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestPackageShouldBeUniqueInPickTrolleySlot

		public void TestPackageShouldBeUniqueInPickTrolleySlot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			var strategy = new WhsPickByTrolleyPickStrategy();
			AssertEquals("Precondition", false, strategy.HasAnyPackageAssignedToATrolleyJob(order));

			var expectedErrorMessage = "Package should be unique";

			var trolley1 = Helper.CreateTrolley("TR1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1, PickTrolleyStatus.Codes.Finalised);
			var package1 = order.PackageJob.Packages.AddNew();
			var slot1_1 = Helper.CreateWhsPickTrolleySlot(trolleyJob1, package1, 1);
			var slot1_2 = Helper.CreateWhsPickTrolleySlot(trolleyJob1, package1, 2);
			AssertNoError("First usage of package1", slot1_1.WTS_KP_PackageInfo, expectedErrorMessage);
			AssertHasError("Package1 is used in slot 1 and is not unique", slot1_2.WTS_KP_PackageInfo, expectedErrorMessage);

			var package2 = order.PackageJob.Packages.AddNew();
			slot1_2.WTS_KP_Package = package2.PK;
			AssertNoError("Package2 is unique and should not have error", slot1_2.WTS_KP_PackageInfo, expectedErrorMessage);

			// check that unique in system, not per trolley
			var trolley2 = Helper.CreateTrolley("TR2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2, PickTrolleyStatus.Codes.Picking);
			var slot2_1 = Helper.CreateWhsPickTrolleySlot(trolleyJob2, package1, 3);
			AssertHasError("Package1 is used in slot 1 in first trolly and is not unique", slot2_1.WTS_KP_PackageInfo, expectedErrorMessage);

			var package3 = order.PackageJob.Packages.AddNew();
			slot2_1.WTS_KP_Package = package3.PK;
			AssertNoError("Package3 is unique and should not have error", slot2_1.WTS_KP_PackageInfo, expectedErrorMessage);
		}

		#endregion
	}
}
