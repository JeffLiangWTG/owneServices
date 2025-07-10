using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsLoadPackageViewTest : WhsTestCaseWithFactory
	{
		public void TestView_PCOHandlingUnit_RTP()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages at packing station.", 1, results.Length);

			var packageRow = results.Where(o => (ZGuid)o["KP_PK"] == package.PK).Single();

			AssertRow(result: packageRow, packagePK: package.PK, handlingUnitPK: handlingUnitPackage.PK, loadPK: load.PK, packageID: "PKG", consolidatedPackingHUPackageID: "HU", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order.WD_ExternalReference, orderPK: order.PK, isLinkedThroughOrder: true);
		}

		public void TestView_PCOHandlingUnit_RTP_TwoOrdersInHU()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Bag);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Pallet);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			PackingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages at packing station.", 2, results.Length);

			var package1Row = results.Where(o => (ZGuid)o["KP_PK"] == package1.PK).Single();
			var package2Row = results.Where(o => (ZGuid)o["KP_PK"] == package2.PK).Single();

			AssertRow(result: package1Row, packagePK: package1.PK, handlingUnitPK: handlingUnitPackage.PK, loadPK: load.PK, packageID: "PKG1", consolidatedPackingHUPackageID: "HU", loadHUPackageID: "", packType: PkgUnit.Bag, topHandlingUnitPackType: PkgUnit.Pallet, orderNumber: order1.WD_ExternalReference, orderPK: order1.PK, isLinkedThroughOrder: true);

			AssertRow(result: package2Row, packagePK: package2.PK, handlingUnitPK: handlingUnitPackage.PK, loadPK: load.PK, packageID: "PKG2", consolidatedPackingHUPackageID: "HU", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Pallet, orderNumber: order2.WD_ExternalReference, orderPK: order2.PK, isLinkedThroughOrder: true);
		}

		public void TestView_PCOHandlingUnit_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WLO_PlannedLoad = load.PK;

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
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages at dockdoor location.", 1, results.Length);

			var packageRow = results.Where(o => (ZGuid)o["KP_PK"] == package.PK).Single();

			AssertRow(result: packageRow, packagePK: package.PK, handlingUnitPK: handlingUnitPackage.PK, loadPK: load.PK, packageID: "PKG", consolidatedPackingHUPackageID: "HU", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order.WD_ExternalReference, orderPK: order.PK, isLinkedThroughOrder: true);
		}

		public void TestView_Package_RTP()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve package at packing station.", 1, results.Length);

			AssertRow(result: results.Single(), packagePK: package.PK, handlingUnitPK: ZGuid.Empty, loadPK: load.PK, packageID: "PKG", consolidatedPackingHUPackageID: "", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: string.Empty, orderNumber: order.WD_ExternalReference, orderPK: order.PK, isLinkedThroughOrder: true);
		}

		public void TestView_Package_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve package at dockdoor location.", 1, results.Length);

			AssertRow(
				result: results.Single(),
				handlingUnitPK: ZGuid.Empty,
				packagePK: package.PK,
				loadPK: load.PK,
				packageID: "PKG",
				consolidatedPackingHUPackageID: "",
				loadHUPackageID: "",
				packType: PkgUnit.Box,
				topHandlingUnitPackType: string.Empty,
				orderNumber: order.WD_ExternalReference,
				orderPK: order.PK,
				isLinkedThroughOrder: true);
		}

		public void TestView_PCOHandlingUnit_OnLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

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

			var package = PackingHelper.CreatePackage(packageJob, "Pkg", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 20m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			var pivotPKG = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivotPKG.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG.WLP_GS_NKLoadingUser = "LTS";

			var pivotHU = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage.PK, load);
			pivotHU.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotHU.WLP_GS_NKLoadingUser = "LTS";
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which have been loaded.", 2, results.Length);

			var handlingUnitRow = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitPackage.PK).Single();
			var packageRow = results.Where(o => (ZGuid)o["KP_PK"] == package.PK).Single();

			AssertRow(result: handlingUnitRow, handlingUnitPK: ZGuid.Empty, packagePK: handlingUnitPackage.PK, loadPK: load.PK, packageID: "HU", consolidatedPackingHUPackageID: "", loadHUPackageID: "", packType: PkgUnit.Package, topHandlingUnitPackType: string.Empty, orderNumber: "", orderPK: ZGuid.Empty, isLinkedThroughOrder: false);

			AssertRow(result: packageRow, handlingUnitPK: handlingUnitPackage.PK, packagePK: package.PK, loadPK: load.PK, packageID: "Pkg", consolidatedPackingHUPackageID: "HU", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order.WD_ExternalReference, orderPK: order.PK, isLinkedThroughOrder: false);
		}

		public void TestView_Package_OnLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var inventoryLine = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob, "PAC", 1, PkgUnit.Package);
			package.Pack(order.Lines[0].ReleaseLines[0], 20m);
			Factory.Save();

			var pivotPKG = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivotPKG.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG.WLP_GS_NKLoadingUser = "~BP";
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which have been loaded.", 1, results.Length);

			var packageRow = results.Where(o => (ZGuid)o["KP_PK"] == package.PK).Single();

			AssertRow(
				result: results.Single(),
				packagePK: package.PK,
				handlingUnitPK: ZGuid.Empty,
				loadPK: load.PK,
				packageID: "PAC",
				consolidatedPackingHUPackageID: "",
				loadHUPackageID: "",
				packType: PkgUnit.Package,
				topHandlingUnitPackType: string.Empty,
				orderNumber: order.WD_ExternalReference,
				orderPK: order.PK,
				isLinkedThroughOrder: false);
		}

		public void TestView_PCOHandlingUnits_OnLoadAndNot()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m);
			order2.WD_WLO_PlannedLoad = load.PK;
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.FinaliseDocketLine();

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);

			var package1 = PackingHelper.CreatePackage(packageJob1, "Box", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package1.KP_GS_NKClosedBy = "LTS";

			var package2 = PackingHelper.CreatePackage(packageJob2, "Basket", 1, PkgUnit.Basket);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);
			package2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package2.KP_GS_NKClosedBy = "LTS";

			var handlingUnit1 = Factory.New<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";

			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "PCOHU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);

			var handlingUnit2 = Factory.New<PkgHandlingUnit>();
			handlingUnit2.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit2.KPU_JobContext = "3PL";

			var handlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit2);
			var handlingUnitPackage2 = PackingHelper.CreatePackage(handlingUnitPackageJob2, "PCOHU2", 1, PkgUnit.Pallet);
			handlingUnitPackage2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage2.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage2, package2, handlingUnitPackage2);
			Factory.Save();

			var pivotPKG = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivotPKG.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG.WLP_GS_NKLoadingUser = "LTS";

			var pivotHU = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage1.PK, load);
			pivotHU.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotHU.WLP_GS_NKLoadingUser = "LTS";
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which are attached to load.", 3, results.Length);

			var handlingUnitRow1 = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitPackage1.PK).Single();
			var packageRow1 = results.Where(o => (ZGuid)o["KP_PK"] == package1.PK).Single();

			var packageRow2 = results.Where(o => (ZGuid)o["KP_PK"] == package2.PK).Single();

			AssertRow(result: handlingUnitRow1, handlingUnitPK: ZGuid.Empty, packagePK: handlingUnitPackage1.PK, loadPK: load.PK, packageID: "PCOHU1", consolidatedPackingHUPackageID: "", loadHUPackageID: "", packType: PkgUnit.Package, topHandlingUnitPackType: string.Empty, orderNumber: "", orderPK: ZGuid.Empty, isLinkedThroughOrder: false);

			AssertRow(result: packageRow1, handlingUnitPK: handlingUnitPackage1.PK, packagePK: package1.PK, loadPK: load.PK, packageID: "Box", consolidatedPackingHUPackageID: "PCOHU1", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order1.WD_ExternalReference, orderPK: order1.PK, isLinkedThroughOrder: false);

			AssertRow(result: packageRow2, handlingUnitPK: handlingUnitPackage2.PK, packagePK: package2.PK, loadPK: load.PK, packageID: "Basket", consolidatedPackingHUPackageID: "PCOHU2", loadHUPackageID: "", packType: PkgUnit.Basket, topHandlingUnitPackType: PkgUnit.Pallet, orderNumber: order2.WD_ExternalReference, orderPK: order2.PK, isLinkedThroughOrder: true);
		}

		public void TestView_PCOHandlingUnits_PackedInLoadHU()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m);
			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.FinaliseDocketLine();

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);

			var package1 = PackingHelper.CreatePackage(packageJob1, "Box", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package1.KP_GS_NKClosedBy = "LTS";

			var package2 = PackingHelper.CreatePackage(packageJob2, "Basket", 1, PkgUnit.Basket);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);
			package2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package2.KP_GS_NKClosedBy = "LTS";

			var handlingUnit1 = Factory.New<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";

			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "PCOHU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "LTS";

			var divot1 = PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);
			divot1.KPD_UnpackedTime = ZDateTimeOffset.UtcNow;
			divot1.KPD_GS_NKUnpackedUser = "LTS";
			package1.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			Factory.Save();
			var divot2 = PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);
			divot2.KPD_UnpackedTime = ZDateTimeOffset.UtcNow;
			divot2.KPD_GS_NKUnpackedUser = "LTS";
			package1.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			Factory.Save();
			var divot3 = PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);

			var handlingUnit2 = Factory.New<PkgHandlingUnit>();
			handlingUnit2.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit2.KPU_JobContext = "3PL";

			var handlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit2);
			var handlingUnitPackage2 = PackingHelper.CreatePackage(handlingUnitPackageJob2, "PCOHU2", 1, PkgUnit.Pallet);
			handlingUnitPackage2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage2.KP_GS_NKClosedBy = "LTS";

			var divot4 = PackingHelper.PackHandlingUnit(handlingUnitPackage2, package2, handlingUnitPackage2);
			divot4.KPD_UnpackedTime = ZDateTimeOffset.UtcNow;
			divot4.KPD_GS_NKUnpackedUser = "LTS";
			package2.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			Factory.Save();
			var divot6 = PackingHelper.PackHandlingUnit(handlingUnitPackage2, package2, handlingUnitPackage2);

			var pivotPKG1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivotPKG1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG1.WLP_GS_NKLoadingUser = "LTS";

			var pivotHU1 = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage1.PK, load);
			pivotHU1.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotHU1.WLP_GS_NKLoadingUser = "LTS";

			var pivotPKG2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			pivotPKG2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG2.WLP_GS_NKLoadingUser = "LTS";

			var pivotHU2 = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage2.PK, load);
			pivotHU2.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotHU2.WLP_GS_NKLoadingUser = "LTS";

			var handlingUnitLoad = Factory.New<PkgHandlingUnit>();
			handlingUnitLoad.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnitLoad.KPU_JobContext = "3PL";

			var handlingUnitLoadPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnitLoad);
			var handlingUnitLoadPackage = PackingHelper.CreatePackage(handlingUnitLoadPackageJob, "LoadHU", 1, PkgUnit.Package);
			Factory.Save();

			PackingHelper.PackHandlingUnit(handlingUnitLoadPackage, handlingUnitPackage1, handlingUnitLoadPackage);
			package1.KP_KP_TopHandlingUnitPackage = handlingUnitLoadPackage.PK;

			var pivotLoadHU = Helper.CreateLoadPkgPackagePivot(handlingUnitLoadPackage.PK, load);
			pivotLoadHU.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotLoadHU.WLP_GS_NKLoadingUser = "LTS";
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which are attached to load.", 5, results.Length);

			var handlingUnitRow1 = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitPackage1.PK).Single();
			var packageRow1 = results.Where(o => (ZGuid)o["KP_PK"] == package1.PK).Single();

			var handlingUnitRow2 = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitPackage2.PK).Single();
			var packageRow2 = results.Where(o => (ZGuid)o["KP_PK"] == package2.PK).Single();

			var handlingUnitLoadPackageRow = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitLoadPackage.PK).Single();

			AssertRow(
				result: handlingUnitRow1,
				packagePK: handlingUnitPackage1.PK,
				handlingUnitPK: handlingUnitLoadPackage.PK,
				loadPK: load.PK,
				packageID: "PCOHU1",
				consolidatedPackingHUPackageID: "",
				loadHUPackageID: "LoadHU",
				packType: PkgUnit.Package,
				topHandlingUnitPackType: PkgUnit.Package,
				orderNumber: "",
				orderPK: ZGuid.Empty,
				isLinkedThroughOrder: false);

			AssertRow(
				result: packageRow1,
				packagePK: package1.PK,
				handlingUnitPK: handlingUnitLoadPackage.PK,
				loadPK: load.PK,
				packageID: "Box",
				consolidatedPackingHUPackageID: "PCOHU1",
				loadHUPackageID: "LoadHU",
				packType: PkgUnit.Box,
				topHandlingUnitPackType: PkgUnit.Package,
				orderNumber: order1.WD_ExternalReference,
				orderPK: order1.PK,
				isLinkedThroughOrder: false);

			AssertRow(
				result: handlingUnitRow2,
				packagePK: handlingUnitPackage2.PK,
				handlingUnitPK: ZGuid.Empty,
				loadPK: load.PK,
				packageID: "PCOHU2",
				consolidatedPackingHUPackageID: "",
				loadHUPackageID: "",
				packType: PkgUnit.Pallet,
				topHandlingUnitPackType: string.Empty,
				orderNumber: "",
				orderPK: ZGuid.Empty,
				isLinkedThroughOrder: false);

			AssertRow(
				result: packageRow2,
				packagePK: package2.PK,
				handlingUnitPK: handlingUnitPackage2.PK,
				loadPK: load.PK,
				packageID: "Basket",
				consolidatedPackingHUPackageID: "PCOHU2",
				loadHUPackageID: "",
				packType: PkgUnit.Basket,
				topHandlingUnitPackType: PkgUnit.Pallet,
				orderNumber: order2.WD_ExternalReference,
				orderPK: order2.PK,
				isLinkedThroughOrder: false);

			AssertRow(
				result: handlingUnitLoadPackageRow,
				packagePK: handlingUnitLoadPackage.PK,
				handlingUnitPK: ZGuid.Empty,
				loadPK: load.PK,
				packageID: "LoadHU",
				consolidatedPackingHUPackageID: "",
				loadHUPackageID: "",
				packType: PkgUnit.Package,
				topHandlingUnitPackType: string.Empty,
				orderNumber: "",
				orderPK: ZGuid.Empty,
				isLinkedThroughOrder: false);
		}

		public void TestView_PCOHandlingUnit_UnclosedAndWithoutLoadPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			order1.WD_WLO_PlannedLoad = load.PK;
			var pick = Helper.CreatePickNew(order1);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);

			var package1 = PackingHelper.CreatePackage(packageJob1, "Box", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package1.KP_GS_NKClosedBy = "LTS";

			var handlingUnit1 = Factory.New<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";

			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "PCOHU1", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which are attached to load.", 1, results.Length);

			var packageRow1 = results.Where(o => (ZGuid)o["KP_PK"] == package1.PK).Single();

			AssertRow(result: packageRow1, handlingUnitPK: handlingUnitPackage1.PK, packagePK: package1.PK, loadPK: load.PK, packageID: "Box", consolidatedPackingHUPackageID: "PCOHU1", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order1.WD_ExternalReference, orderPK: order1.PK, isLinkedThroughOrder: true);
		}

		public void TestView_PCOHandlingUnit_ClosedAndWithLoadPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			order1.WD_WLO_PlannedLoad = load.PK;
			var pick = Helper.CreatePickNew(order1);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);

			var package1 = PackingHelper.CreatePackage(packageJob1, "Box", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package1.KP_GS_NKClosedBy = "LTS";

			var handlingUnit1 = Factory.New<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";

			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "PCOHU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "LTS";
			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);
			Factory.Save();

			var pivotPKG = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivotPKG.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotPKG.WLP_GS_NKLoadingUser = "LTS";

			var pivotHU = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage1.PK, load);
			pivotHU.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotHU.WLP_GS_NKLoadingUser = "LTS";
			Factory.Save();

			var handlingUnitLoad = Factory.New<PkgHandlingUnit>();
			handlingUnitLoad.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnitLoad.KPU_JobContext = "3PL";

			var handlingUnitLoadPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnitLoad);
			var handlingUnitLoadPackage = PackingHelper.CreatePackage(handlingUnitLoadPackageJob, "LoadHU", 1, PkgUnit.Package);
			Factory.Save();

			PackingHelper.PackHandlingUnit(handlingUnitLoadPackage, handlingUnitPackage1, handlingUnitLoadPackage);
			package1.KP_KP_TopHandlingUnitPackage = handlingUnitLoadPackage.PK;

			var pivotLoadHU = Helper.CreateLoadPkgPackagePivot(handlingUnitLoadPackage.PK, load);
			pivotLoadHU.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotLoadHU.WLP_GS_NKLoadingUser = "LTS";
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which are attached to load.", 3, results.Length);

			var handlingUnitRow1 = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitPackage1.PK).Single();
			var packageRow1 = results.Where(o => (ZGuid)o["KP_PK"] == package1.PK).Single();
			var handlingUnitLoadPackageRow = results.Where(o => (ZGuid)o["KP_PK"] == handlingUnitLoadPackage.PK).Single();

			AssertRow(result: handlingUnitRow1, handlingUnitPK: handlingUnitLoadPackage.PK, packagePK: handlingUnitPackage1.PK, loadPK: load.PK, packageID: "PCOHU1", consolidatedPackingHUPackageID: "", loadHUPackageID: "LoadHU", packType: PkgUnit.Package, topHandlingUnitPackType: PkgUnit.Package, orderNumber: "", orderPK: ZGuid.Empty, isLinkedThroughOrder: false);
			AssertRow(result: packageRow1, handlingUnitPK: handlingUnitLoadPackage.PK, packagePK: package1.PK, loadPK: load.PK, packageID: "Box", consolidatedPackingHUPackageID: "PCOHU1", loadHUPackageID: "LoadHU", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order1.WD_ExternalReference, orderPK: order1.PK, isLinkedThroughOrder: false);
			AssertRow(result: handlingUnitLoadPackageRow, handlingUnitPK: ZGuid.Empty, packagePK: handlingUnitLoadPackage.PK, loadPK: load.PK, packageID: "LoadHU", consolidatedPackingHUPackageID: "", loadHUPackageID: "", packType: PkgUnit.Package, topHandlingUnitPackType: string.Empty, orderNumber: "", orderPK: ZGuid.Empty, isLinkedThroughOrder: false);
		}

		public void TestView_PCOHandlingUnit_ClosedAndWithoutLoadPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			order1.WD_WLO_PlannedLoad = load.PK;
			var pick = Helper.CreatePickNew(order1);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);

			var package1 = PackingHelper.CreatePackage(packageJob1, "Box", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package1.KP_GS_NKClosedBy = "LTS";

			var handlingUnit1 = Factory.New<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";

			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "PCOHU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "LTS";
			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve packages which are attached to load.", 1, results.Length);

			var packageRow1 = results.Where(o => (ZGuid)o["KP_PK"] == package1.PK).Single();

			AssertRow(result: packageRow1, handlingUnitPK: handlingUnitPackage1.PK, packagePK: package1.PK, loadPK: load.PK, packageID: "Box", consolidatedPackingHUPackageID: "PCOHU1", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order1.WD_ExternalReference, orderPK: order1.PK, isLinkedThroughOrder: true);
		}

		public void TestView_InnerPackagesIgnored()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			order.WD_WLO_PlannedLoad = load.PK;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = PackingHelper.CreatePackage(packageJob1, "Box", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var inner1 = package.Packages.AddNew(PkgUnit.Bag, "Bag");
			inner1.Pack(order.Lines[0].ReleaseLines[0], 10m);
			inner1.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			var inner2 = package.Packages.AddNew(PkgUnit.Bottle, "Bot");
			inner2.Pack(order.Lines[0].ReleaseLines[0], 5m);
			inner2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			var handlingUnit1 = Factory.New<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";

			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "PCOHU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "LTS";
			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package, handlingUnitPackage1);
			Factory.Save();

			var results = GetAllPackages();
			AssertEquals("Should retrieve outer packages which are attached to load.", 1, results.Length);

			var packageRow1 = results.Where(o => (ZGuid)o["KP_PK"] == package.PK).Single();

			AssertRow(result: packageRow1, handlingUnitPK: handlingUnitPackage1.PK, packagePK: package.PK, loadPK: load.PK, packageID: "Box", consolidatedPackingHUPackageID: "PCOHU1", loadHUPackageID: "", packType: PkgUnit.Box, topHandlingUnitPackType: PkgUnit.Package, orderNumber: order.WD_ExternalReference, orderPK: order.PK, isLinkedThroughOrder: true);
		}

		#region AssertRow

		void AssertRow(
			DynamicBusinessObject result,
			ZGuid packagePK,
			ZGuid handlingUnitPK,
			ZGuid loadPK,
			string packageID,
			string consolidatedPackingHUPackageID,
			string loadHUPackageID,
			string packType,
			string topHandlingUnitPackType,
			string orderNumber,
			ZGuid orderPK,
			bool isLinkedThroughOrder)
		{
			AssertEquals("KP_PK", packagePK, result["KP_PK"]);
			AssertEquals("KP_PKAsFK", packagePK, result["KP_PKAsFK"]);
			AssertEquals("KP_KP_TopHandlingUnitPackage", handlingUnitPK, result["KP_KP_TopHandlingUnitPackage"]);
			AssertEquals("WhsLoadPK", loadPK, result["WhsLoadPK"]);
			AssertEquals("KPH_PackageID", packageID, result["KPH_PackageID"]);
			AssertEquals("ConsolidatedPackingHUPackageID", consolidatedPackingHUPackageID, result["ConsolidatedPackingHUPackageID"]);
			AssertEquals("LoadHUPackageID", loadHUPackageID, result["LoadHUPackageID"]);
			AssertEquals("KP_F3_NKPackType", packType, result["KP_F3_NKPackType"]);
			AssertEquals("TopHandlingUnitPackType", topHandlingUnitPackType, result["TopHandlingUnitPackType"]);
			AssertEquals("OrderNumber", orderNumber, result["OrderNumber"]);
			AssertEquals("WD_PKAsFK", orderPK, result["WD_PKAsFK"]);
			AssertEquals("isLinkedThroughOrder", isLinkedThroughOrder, result["isLinkedThroughOrder"]);
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

		DynamicBusinessObject[] GetAllPackages()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = $"select * from dbo.WhsLoadPackage";

			AssertNoExceptionThrown(() => result.Load(sql));
			return result.ToArray();
		}

		#endregion
	}
}
