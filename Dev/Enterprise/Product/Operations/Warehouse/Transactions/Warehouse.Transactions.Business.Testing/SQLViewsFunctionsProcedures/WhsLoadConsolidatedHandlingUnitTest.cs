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
	public class WhsLoadConsolidatedHandlingUnitTest : WhsTestCaseWithFactory
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

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			Factory.Save();

			AssertEquals("Should retrieve No packages at packing station.", 0, GetAllUnits().Length);
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

			var results = GetAllUnits();
			AssertEquals("Should retrieve HUs at dockdoor location.", 1, results.Length);

			AssertRow(
				result: results.Single(),
				packagePK: handlingUnitPackage.PK,
				loadPK: load.PK);
		}

		public void TestView_PCOHandlingUnit_Staged_OpenHU()
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
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			AssertEquals("Should retrieve No open HU packages.", 0, GetAllUnits().Length);
		}

		public void TestView_PCOHandlingUnit_Staged_NoLoadCore()
		{
			TestView_PCOHandlingUnit_Staged_NoLoadCore(hasOtherLoad: false);
		}

		public void TestView_PCOHandlingUnit_Staged_OtherLoad()
		{
			TestView_PCOHandlingUnit_Staged_NoLoadCore(hasOtherLoad: true);
		}

		void TestView_PCOHandlingUnit_Staged_NoLoadCore(bool hasOtherLoad)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck1 = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck1, startTime: DateTimeOffset.Now);
			var truck2 = Helper.CreateEquipment("T002", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L2", transportUnit: truck2, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			if (hasOtherLoad)
			{
				order.WD_WLO_PlannedLoad = load2.PK;
			}

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
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

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";
			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = $"select * from dbo.WhsLoadConsolidatedHandlingUnit where WLC_LoadPK = '{load1.PK}'";

			AssertNoExceptionThrown(() => result.Load(sql));
			AssertEquals("Should retrieve No HU packages for empty load or another load.", 0, result.ToArray().Length);
		}

		public void TestView_PCOHandlingUnit_Empty()
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
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";
			Factory.Save();

			AssertEquals("Should retrieve No empty HU packages.", 0, GetAllUnits().Length);
		}

		public void TestView_PCOHandlingUnit_Loaded()
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
			order.WD_WLO_PlannedLoad = load.PK;

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

			AssertEquals("Should retrieve No packages at packing station.", 0, GetAllUnits().Length);
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
			order1.WD_WLO_PlannedLoad = load.PK;
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
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);

			var handlingUnit2 = Factory.New<PkgHandlingUnit>();
			handlingUnit2.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit2.KPU_JobContext = "3PL";

			var handlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit2);
			var handlingUnitPackage2 = PackingHelper.CreatePackage(handlingUnitPackageJob2, "HU2", 1, PkgUnit.Pallet);
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

			var results = GetAllUnits();
			AssertEquals("Should retrieve not loaded HUs at dockdoor location.", 1, results.Length);

			AssertRow(
				result: results.Single(),
				packagePK: handlingUnitPackage2.PK,
				loadPK: load.PK);
		}

		public void TestView_PCOHandlingUnit_UnLoaded()
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

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			order.WD_WLO_PlannedLoad = load.PK;

			Helper.CreatePickNew(order);

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
			pivotPKG.WLP_UnloadedTime = ZDateTimeOffset.Now;
			pivotPKG.WLP_GS_NKUnloadingUser = "LTS";

			var pivotHU = Helper.CreateLoadPkgPackagePivot(handlingUnitPackage.PK, load);
			pivotHU.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivotHU.WLP_GS_NKLoadingUser = "LTS";
			pivotHU.WLP_UnloadedTime = ZDateTimeOffset.Now;
			pivotHU.WLP_GS_NKUnloadingUser = "LTS";
			Factory.Save();

			var results = GetAllUnits();
			AssertEquals("Should retrieve unloaded HUs at dockdoor location.", 1, results.Length);

			AssertRow(
				result: results.Single(),
				packagePK: handlingUnitPackage.PK,
				loadPK: load.PK);
		}

		#region AssertRow

		void AssertRow(
			DynamicBusinessObject result,
			ZGuid packagePK,
			ZGuid loadPK)
		{
			AssertEquals("WLC_HUPackagePK", packagePK, result["WLC_HUPackagePK"]);
			AssertEquals("WLC_PK", packagePK, result["WLC_PK"]);
			AssertEquals("WLC_LoadPK", loadPK, result["WLC_LoadPK"]);
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

		DynamicBusinessObject[] GetAllUnits()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = $"select * from dbo.WhsLoadConsolidatedHandlingUnit";

			AssertNoExceptionThrown(() => result.Load(sql));
			return result.ToArray();
		}

		#endregion
	}
}
