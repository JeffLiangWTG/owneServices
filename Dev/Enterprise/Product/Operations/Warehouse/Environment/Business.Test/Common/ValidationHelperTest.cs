using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class ValidationHelperTest : WhsTestCaseWithFactoryEnv
	{
		#region TestValidationHelper_CheckIfLocationsHaveCustomsStockOnHand

		public void TestValidationHelper_CheckIfLocationsHaveCustomsStockOnHand()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var bondedArea = (WhsArea)transactionTestHelper.CreateWhsArea(data.Whs1.PK, "BONDED", "BON");
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PickingArea = bondedArea.PK;
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			var location3 = data.Whs1.FindLocation("A-3");
			location3.WLV_WA_PickingArea = bondedArea.PK;
			location3.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);

			var customsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", "CUS", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(customsReceivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-2");
			transactionTestHelper.CreateWhsReceiveInventoryLine(customsReceivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-2", "A-3");
			transactionTestHelper.FinaliseDocket(customsReceivePK);
			Factory.Save();

			var location1PK = data.Whs1.FindLocation("A-1").PK;
			var location2PK = location2.PK;
			var location3PK = location3.PK;
			var location4PK = data.Whs1.FindLocation("A-4").PK;

			AssertEquals("2 Locations with SOH. 1 with Customs.", true, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, location1PK, location2PK));
			AssertEquals("1 Location with Customs SOH, 1 location with goods SOH.", true, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, location1PK, location3PK));
			AssertEquals("1 Location with Customs SOH, 1 location with no SOH.", true, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, location3PK, location4PK));
			AssertEquals("2 locations with Customs SOH.", true, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, location2PK, location3PK));
			AssertEquals("1 Location with Customs SOH, 1 locations with SOH, 1 with no SOH.", true, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, new ZGuid[] { location1PK, location3PK, location4PK }));
			AssertEquals("1 Location with Customs SOH.", true, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, location2PK));
			AssertEquals("1 Location with normal SOH.", false, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, location1PK));
			AssertEquals("1 locations with normal SOH. 1 with no SOH", false, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, new ZGuid[] { location1PK, location4PK }));
			AssertEquals("1 location with no SOH.", false, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, new ZGuid[] { location4PK }));
			AssertEquals("No locations", false, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory, Array.Empty<ZGuid>()));
			AssertEquals("No locations", false, ValidationHelper.CheckIfLocationsHaveCustomsStockOnHand(Factory));
		}

		#endregion

		#region TestValidationHelper_CheckIfLocationsHaveStockOnHand

		public void TestValidationHelper_CheckIfLocationsHaveStockOnHand()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var bondedArea = (WhsArea)transactionTestHelper.CreateWhsArea(data.Whs1.PK, "BONDED", "BON");
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PickingArea = bondedArea.PK;
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			var location3 = data.Whs1.FindLocation("A-3");
			location3.WLV_WA_PickingArea = bondedArea.PK;
			location3.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-1");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);

			var customsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", "CUS", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(customsReceivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-2");
			transactionTestHelper.CreateWhsReceiveInventoryLine(customsReceivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-2", "A-3");
			transactionTestHelper.FinaliseDocket(customsReceivePK);
			Factory.Save();

			var location1PK = data.Whs1.FindLocation("A-1").PK;
			var location2PK = location2.PK;
			var location3PK = location3.PK;
			var location4PK = data.Whs1.FindLocation("A-4").PK;

			AssertEquals("2 Locations with SOH. 1 with Customs.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, location1PK, location2PK));
			AssertEquals("1 Location with Customs SOH, 1 location with goods SOH.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, location1PK, location3PK));
			AssertEquals("1 Location with Customs SOH, 1 location with no SOH.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, location3PK, location4PK));
			AssertEquals("2 locations with Customs SOH.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, location2PK, location3PK));
			AssertEquals("1 Location with Customs SOH, 1 locations with SOH, 1 with no SOH.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, new ZGuid[] { location1PK, location3PK, location4PK }));
			AssertEquals("1 Location with Customs SOH.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, location2PK));
			AssertEquals("1 Location with normal SOH.", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, location1PK));
			AssertEquals("1 locations with normal SOH. 1 with no SOH", true, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, new ZGuid[] { location1PK, location4PK }));
			AssertEquals("1 location with no SOH.", false, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, new ZGuid[] { location4PK }));
			AssertEquals("No locations", false, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory, Array.Empty<ZGuid>()));
			AssertEquals("No locations", false, ValidationHelper.CheckIfLocationsHaveStockOnHand(Factory));
		}

		#endregion

		#region TestValidationHelper_CheckIfLocationsHaveTransitPackage

		public void TestValidationHelper_CheckIfLocationsHaveTransitPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS", "A", 5, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);
			Factory.Save();

			var dockDoor = warehouse.FindLocation("DOCK");
			var location1PK = warehouse.FindLocation("A-1").PK;
			var location2PK = warehouse.FindLocation("A-2").PK;
			var location3PK = warehouse.FindLocation("A-3").PK;
			Factory.Save();

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, dockDoor.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = dockDoor.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package1 = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location1PK;
			packageState[WhsItemPackageStateSchema.WPS_Status] = "ARV";
			packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package1.PK;
			packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = warehouse.PK;
			packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			packageState[WhsItemPackageStateSchema.WPS_SecurityStatus] = "REQ";

			var package2 = (BusinessObject)Helper.CreatePackage(packageJob);
			var handlingUnitPackage = (BusinessObject)Factory.New<IWhsItemPackageState>();
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location2PK;
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_Status] = "ARV";
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_KP_Package] = package2.PK;
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_WW_Warehouse] = warehouse.PK;
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_IsHandlingUnit] = true;
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			handlingUnitPackage[WhsItemPackageStateSchema.WPS_SecurityStatus] = "REQ";

			var rcn = (BusinessObject)Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = (BusinessObject)Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = (BusinessObject)Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dispatchLoadList = (BusinessObject)Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var package3 = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState3 = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState3[WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment] = rcn.PK;
			packageState3[WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment] = dcn.PK;
			packageState3[WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader] = dtu.PK;
			packageState3[WhsItemPackageStateSchema.WPS_WDL_LoadList] = dispatchLoadList.PK;
			packageState3[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location3PK;
			packageState3[WhsItemPackageStateSchema.WPS_Status] = "DEP";
			packageState3[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState3[WhsItemPackageStateSchema.WPS_KP_Package] = package3.PK;
			packageState3[WhsItemPackageStateSchema.WPS_WW_Warehouse] = warehouse.PK;
			packageState3[WhsItemPackageStateSchema.WPS_IsSecure] = true;
			packageState3[WhsItemPackageStateSchema.WPS_SecurityStatus] = "SEC";
			packageState3[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			packageState3[WhsItemPackageStateSchema.WPS_LoadedTime] = ZDateTimeOffset.Now;

			Factory.Save();

			AssertEquals("1 Locations with arrived package, 1 Location with handling unit package, 1 location with dispatched package.",
				true, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location1PK, location2PK, location3PK));
			AssertEquals("1 Location with package, 1 location with dispatched package.", true, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location1PK, location3PK));
			AssertEquals("1 Location with handling unit package, 1 location with dispatched package.", false, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location2PK, location3PK));
			AssertEquals("1 Locations with package, 1 location with handling unit package.", true, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location1PK, location2PK));
			AssertEquals("1 Locations with package.", true, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location1PK));
			AssertEquals("1 Location with handling unit package.", false, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location2PK));
			AssertEquals("1 Location with dispatched package.", false, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, location3PK));
			AssertEquals("No locations", false, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory, Array.Empty<ZGuid>()));
			AssertEquals("No locations", false, ValidationHelper.CheckIfLocationsHaveTransitPackage(Factory));
		}

		#endregion
	}
}
