using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Packing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderPackageStrategyTest : WhsTestCaseWithFactory
	{
		#region TestGetPackageActionStrategy

		public void TestGetPackageActionStrategy_PackageNotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			AssertEquals("Precondition: package is not saved to DB.", false, package.IsInDatabase);

			var packageActionStrategy = new WhsOrderPackageStrategy().GetPackageActionStrategy(order, package);

			AssertNull("Strategy should be null.", packageActionStrategy);
		}

		public void TestGetPackageActionStrategy_PackageDoesnotHasLoadedPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			Factory.Save();

			AssertEquals("Precondition: package is saved to DB.", true, package.IsInDatabase);
			AssertEquals("Precondition: package doesn't has Loaded Pivots.", false, package.HasLoadedPivot());

			var packageActionStrategy = new WhsOrderPackageStrategy().GetPackageActionStrategy(order, package);

			AssertNull("Strategy should be null.", packageActionStrategy);
		}

		public void TestGetPackageActionStrategy_PackageHasLoadedPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("Precondition: package is saved to DB.", true, package.IsInDatabase);
			AssertEquals("Precondition: package has Loaded Pivots.", true, package.HasLoadedPivot());

			var packageActionStrategy = new WhsOrderPackageStrategy().GetPackageActionStrategy(order, package);

			AssertNotNull("Strategy should not be null.", packageActionStrategy);
			AssertEquals("IsActionAllowed.", true, packageActionStrategy.IsActionAllowed(PackageAction.Edit));
			AssertEquals("IsActionAllowed.", false, packageActionStrategy.IsActionAllowed(PackageAction.Delete));
			AssertEquals("IsActionAllowed.", false, packageActionStrategy.IsActionAllowed(PackageAction.PackUnpack));
			AssertEquals("Cannot delete or unpack a loaded package.", packageActionStrategy.ReasonForNotAllowingAction);
			AssertEquals(package, packageActionStrategy.Package);
		}

		#endregion
	}
}
