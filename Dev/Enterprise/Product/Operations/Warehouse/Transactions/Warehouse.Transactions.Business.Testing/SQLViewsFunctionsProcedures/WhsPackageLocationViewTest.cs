using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPackageLocationViewTest : WhsTestCaseWithFactory
	{
		public void TestView_PackedHandlingUnitAtConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Factory.Save();

				var results = GetAllPackages();
				AssertEquals("Should retrieve packages at packing consolidation location.", 2, results.Length);

			var handlingUnitRow = results.Where(o => o.WPK_KP_Package == handlingUnitPackage.PK).Single();
			var packageRow = results.Where(o => o.WPK_KP_Package == package.PK).Single();

			AssertRow(
				handlingUnitRow,
				handlingUnitPackage.PK,
				PkgUnit.Package,
				"HU",
				packingLocation.PK,
				data.Whs1.PK,
				"PS1",
				"PS1",
				"CON",
				true,
				"RTP");

			AssertRow(
				packageRow,
				package.PK,
				PkgUnit.Box,
				"PKG",
				packingLocation.PK,
				data.Whs1.PK,
				"PS1",
				"PS1",
				"CON",
				false,
				"RTP");
		}

		public void TestView_PackedHandlingUnitAtDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages at dockdoor location.", 2, results.Length);

			var handlingUnitRow = results.Where(o => o.WPK_KP_Package == handlingUnitPackage.PK).Single();
			var packageRow = results.Where(o => o.WPK_KP_Package == package.PK).Single();

			AssertRow(
				handlingUnitRow,
				handlingUnitPackage.PK,
				PkgUnit.Package,
				"HU",
				data.Whs1.DefaultOutboundDockDoorLocation.PK,
				data.Whs1.PK,
				"DOCKDOOR",
				"DOCKDOOR",
				"DDL",
				true,
				"STA");

			AssertRow(
				packageRow,
				package.PK,
				PkgUnit.Box,
				"PKG",
				data.Whs1.DefaultOutboundDockDoorLocation.PK,
				data.Whs1.PK,
				"DOCKDOOR",
				"DOCKDOOR",
				"DDL",
				false,
				"STA");
		}

		public void TestView_PackageAtConsolidationLocation()
		{
			TestView_PackageAtConsolidationLocationCore(isOutboundTransferFinalised: true);
		}

		public void TestView_PackageInTransitToConsolidationLocation()
		{
			TestView_PackageAtConsolidationLocationCore(isOutboundTransferFinalised: false);
		}

		void TestView_PackageAtConsolidationLocationCore(bool isOutboundTransferFinalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			if (isOutboundTransferFinalised)
			{
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve package at packing consolidation location.", 1, results.Length);

			AssertRow(
				results.Single(),
				package.PK,
				PkgUnit.Box,
				"PKG",
				packingLocation.PK,
				data.Whs1.PK,
				"PS1",
				"PS1",
				"CON",
				false,
				isOutboundTransferFinalised ? "RTP" : "INT");
		}

		public void TestView_PackageAtDockDoorLocation()
		{
			TestView_PackageAtDockDoorLocationCore(isOutboundTransferFinalised: true);
		}

		public void TestView_PackageInTransitToDockDoorLocation()
		{
			TestView_PackageAtDockDoorLocationCore(isOutboundTransferFinalised: false);
		}

		void TestView_PackageAtDockDoorLocationCore(bool isOutboundTransferFinalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			if (isOutboundTransferFinalised)
			{
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve package at dockdoor location.", 1, results.Length);

			AssertRow(
				results.Single(),
				package.PK,
				PkgUnit.Box,
				"PKG",
				data.Whs1.DefaultOutboundDockDoorLocation.PK,
				data.Whs1.PK,
				"DOCKDOOR",
				"DOCKDOOR",
				"DDL",
				false,
				isOutboundTransferFinalised ? "STA" : "INT");
		}

		public void TestView_EmptyHandlingUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve handling units with no packed inventory.", 1, results.Length);
			AssertRow(
				results.Single(),
				handlingUnitPackage.PK,
				PkgUnit.Package,
				"HU",
				ZGuid.Empty,
				data.Whs1.PK,
				"",
				"",
				"",
				true,
				"");
		}

		public void TestView_InvalidWarehouse_InactiveWarehouse()
		{
			TestView_InvalidWarehouseCore((WhsWarehouse whs) => whs.WW_IsActive = false);
		}

		public void TestView_InvalidWarehouse_VirtualWarehouse()
		{
			TestView_InvalidWarehouseCore((WhsWarehouse whs) => whs.WW_IsVirtualWarehouse = true);
		}

		public void TestView_InvalidWarehouse_NonProductWarehouse()
		{
			TestView_InvalidWarehouseCore((WhsWarehouse whs) => whs.WW_WarehouseType = "FTZ");
		}

		void TestView_InvalidWarehouseCore(Action<WhsWarehouse> updateWarehouseToBeInvalid)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Factory.Save();

			updateWarehouseToBeInvalid(data.Whs1);
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve no packages as warehouse is invalid.", 0, results.Length);
		}

		public void TestView_DuplicateEmptyHandlingUnitsInDifferentWarehouses()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var whs2 = Helper.CreateWarehouse("WH2");
			var whs3 = Helper.CreateWarehouse("WH3");
			whs3.WW_IsVirtualWarehouse = true;
			var whs4 = Helper.CreateWarehouse("WH4");
			whs4.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var handlingUnitPackage1 = CreateHandlingUnitForWarehouse(whs1);
			var handlingUnitPackage2 = CreateHandlingUnitForWarehouse(whs2);
			var handlingUnitPackage3 = CreateHandlingUnitForWarehouse(whs3);
			var handlingUnitPackage4 = CreateHandlingUnitForWarehouse(whs4);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should only retrieve handling units in Physical Product Warehouses.", 2, results.Length);

			var row1 = results.Where(r => r.WPK_WW_Whs == whs1.PK).Single();
			AssertRow(
				row1,
				handlingUnitPackage1,
				PkgUnit.Package,
				"HU",
				ZGuid.Empty,
				whs1.PK,
				"",
				"",
				"",
				true,
				"");

			var row2 = results.Where(r => r.WPK_WW_Whs == whs2.PK).Single();
			AssertRow(
				row2,
				handlingUnitPackage2,
				PkgUnit.Package,
				"HU",
				ZGuid.Empty,
				whs2.PK,
				"",
				"",
				"",
				true,
				"");

			ZGuid CreateHandlingUnitForWarehouse(WhsWarehouse warehouse)
			{
				var handlingUnit = Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_GB_Branch = warehouse.WW_GB_RelatedCompanyBranch;
				handlingUnit.KPU_JobContext = "3PL";

				var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
				var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

				return handlingUnitPackage.PK;
			}
		}

		public void TestView_IgnoresDepartedHandlingUnitAndPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "Pkg", 1, PkgUnit.Package);
			package.Pack(order.Lines[0].ReleaseLines[0], 20m);

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();

			Factory.Save();

			AssertEquals(0m, inventoryLine.WE_StockOnHand);

			var results = GetAllPackages();
			AssertEquals("Should not retrieve packages which have departed.", 0, results.Length);
		}

		#region AssertRow

		void AssertRow(
			WhsPackageLocationView result,
			ZGuid handlingUnitPK,
			string packType,
			string packageID,
			ZGuid locationPK,
			ZGuid warehousePK,
			string locationString,
			string locationString_UserFriendly,
			string locationClass,
			bool isHandlingUnit,
			string packedInventoryStatus)
		{
			AssertEquals("WPK_KP_Package", handlingUnitPK, result.WPK_KP_Package);
			AssertEquals("WPK_F3_NKPackType", packType, result.WPK_F3_NKPackType);
			AssertEquals("WPK_PackageID", packageID, result.WPK_PackageID);
			AssertEquals("WPK_WL_Location", locationPK, result.WPK_WL_Location);
			AssertEquals("WPK_WW_Whs", warehousePK, result.WPK_WW_Whs);
			AssertEquals("WPK_LocationString", locationString, result.WPK_LocationString);
			AssertEquals("WPK_LocationString_UserFriendly", locationString_UserFriendly, result.WPK_LocationString_UserFriendly);
			AssertEquals("WPK_LocationClass", locationClass, result.WPK_LocationClass);
			AssertEquals("WPK_IsHandlingUnit", isHandlingUnit, result.WPK_IsHandlingUnit);
			AssertEquals("WPK_PackedInventoryStatus", packedInventoryStatus, result.WPK_PackedInventoryStatus);
		}

		#endregion

		#region Implementation

		protected PackingTestHelper PackingHelper
		{
			get
			{
				return packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
			}
		}

		PackingTestHelper packingHelper;

		#endregion

		#region LoadSQLFunction

		WhsPackageLocationView[] GetAllPackages()
		{
			return Factory.Load<WhsPackageLocationView>(new ZQuery());
		}

		#endregion
	}
}
