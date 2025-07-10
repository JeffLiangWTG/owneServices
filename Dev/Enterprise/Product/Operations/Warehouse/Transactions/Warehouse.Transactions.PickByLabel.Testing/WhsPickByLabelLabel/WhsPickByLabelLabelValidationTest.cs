namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	using CargoWise.Types;
	using Enterprise.Warehouse.Transactions.Business.Testing;

	class WhsPickByLabelLabelValidationTest : WhsTestCaseWithFactory
	{
		public void TestDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			Factory.Save();

			var expectedError = "Package '{0}' is assigned to a different dock door location than previous labels.";

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertNoError("Adding package for same DDL should not have error", pickByLabelLabel.WTL_KP_PackageInfo, string.Format(expectedError, package.KP_PackageID));

			pick.WP_WL_DockDoor = ZGuid.NewZGuid();
			pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertHasError("Adding package for different DDL should have error.", pickByLabelLabel.WTL_KP_PackageInfo, string.Format(expectedError, package.KP_PackageID));

			pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			pickByLabelLabel.WTL_KP_Package = ZGuid.Invalid;
			AssertHasError("Should not throw exception when package is not set.", pickByLabelLabel.WTL_KP_PackageInfo, string.Format(expectedError, ""));
		}

		public void TestPackingStationLocation_SamePackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();
			var pstLocation = ZGuid.NewZGuid();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertNoErrors("Adding package for same DDL should not have error", pickByLabelLabel.WTL_KP_PackageInfo);

			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);

			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			Factory.Save();
			pick.WP_WL_PackingStation = pstLocation;
			pick2.WP_WL_PackingStation = pstLocation;
			var pickByLabelLabel2 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package2.PK);
			AssertNoErrors("Adding package for same PST should not have error", pickByLabelLabel2.WTL_KP_PackageInfo);
		}

		public void TestPackingStationLocation_DifferentPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertNoErrors("Adding package for same DDL should not have error", pickByLabelLabel.WTL_KP_PackageInfo);

			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);

			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			Factory.Save();
			pick.WP_WL_PackingStation = ZGuid.NewZGuid();
			pick2.WP_WL_PackingStation = ZGuid.NewZGuid();
			var pickByLabelLabel2 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package2.PK);
			var expectedError = "Package '{0}' is assigned to a different packing station location than previous labels.";
			AssertHasError("Adding package for different PST should have error", pickByLabelLabel2.WTL_KP_PackageInfo, string.Format(expectedError, package2.KP_PackageID));
		}

		public void TestPackingStationLocation_DifferentPackingStation_NewPackageWithEmptyPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();
			var pstLocation = ZGuid.NewZGuid();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertNoErrors("Adding package for same DDL should not have error", pickByLabelLabel.WTL_KP_PackageInfo);

			Factory.Save();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);

			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			Factory.Save();
			pick.WP_WL_PackingStation = pstLocation;
			AssertEquals("Precondition: pick2 have empty Packing Station", ZGuid.Empty, pick2.WP_WL_PackingStation);
			var pickByLabelLabel2 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package2.PK);
			var expectedError = "Package '{0}' is assigned to a different packing station location than previous labels.";
			AssertHasError("Adding package without PST to a job with PST should have error", pickByLabelLabel2.WTL_KP_PackageInfo, string.Format(expectedError, package2.KP_PackageID));
		}
	}
}
