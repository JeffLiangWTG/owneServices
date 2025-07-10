using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsAreaValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWA_WW_Whs

		public void TestCheckWA_WW_Whs()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 1);

			var area = whs1.Areas[0];
			area.WA_WW_Whs = whs2.PK;
			AssertHasError(area.WA_WW_WhsInfo, WhsAreaValidation.LocationsAttachedFromAnotherWarehouseErrorMsg);

			area.WA_WW_Whs = whs1.PK;
			AssertNoError(area.WA_WW_WhsInfo, WhsAreaValidation.LocationsAttachedFromAnotherWarehouseErrorMsg);
		}

		public void TestCheckWA_WW_Whs_UpdateToNewWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");

			var area = Helper.CreateArea(whs1, "A");
			AssertEquals("Pre-condtion: Area is not saved yet.", false, area.IsInDatabase);
			area.WA_WW_Whs = whs2.PK;
			AssertNoError("Area is not saved, should allow to change warehouse.", area.WA_WW_WhsInfo, "Cannot update Warehouse for existing Areas.");

			area.WA_WW_Whs = whs1.PK;
			Factory.Save();

			AssertEquals("Pre-condtion: Area is not saved.", true, area.IsInDatabase);
			area.WA_WW_Whs = whs2.PK;
			AssertHasError("Area is saved, should NOT allow to change warehouse.", area.WA_WW_WhsInfo, "Cannot update Warehouse for existing Areas.");
		}

		#endregion

		#region TestCheckWA_Name

		public void TestCheckWA_Name()
		{
			TestMandatoryString(Area.WA_NameInfo, ErrorCheckType.HasErrors);
		}

		#endregion

		#region TestCheckWA_AreaType

		public void TestCheckWA_AreaType()
		{
			TestCodePairList(Area.WA_AreaTypeInfo, ErrorCheckType.HasErrors, false, Area.Lookups.AreaTypes);
		}

		#region TestWA_AreaType_DynamicContainsOnlyDynamicLocations

		public void TestWA_AreaType_DynamicContainsOnlyDynamicLocations()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");

			var area = Factory.New<WhsArea>();
			area.WA_AreaType = AreaTypes.Codes.FreeStore;
			area.WA_IsPickingArea = true;

			var fixLoc = Factory.New<WhsLocationType>();
			fixLoc.WLT_Code = LocationClasses.Codes.FIX;
			fixLoc.WLT_IsActive = true;
			fixLoc.WLT_Description = LocationClasses.Descriptions.FIX;
			fixLoc.WLT_IsSystem = true;
			fixLoc.WLT_MaximumNumberOfProducts = 1;
			fixLoc.WLT_LocationClass = LocationClasses.Codes.FIX;

			var location = Factory.New<WhsLocation>();
			location.WLV_WLT_LocationType = fixLoc.PK;
			location.WLV_WA_PickingArea = area.PK;

			AssertNoErrors("Precondition", area.WA_AreaTypeInfo);

			area.WA_AreaType = AreaTypes.Codes.DynamicPickFace;

			AssertHasError(area.WA_AreaTypeInfo, "Dynamic Pick Face Areas must only contain locations with DPF Location Class.");
		}

		public void TestWA_AreaType_FreeStoreCanContainDynamicLocations()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");

			var area = Factory.New<WhsArea>();
			area.WA_AreaType = AreaTypes.Codes.FreeStore;
			area.WA_IsPickingArea = true;

			var fixLoc = Factory.New<WhsLocationType>();
			fixLoc.WLT_Code = LocationClasses.Codes.FIX;
			fixLoc.WLT_IsActive = true;
			fixLoc.WLT_Description = LocationClasses.Descriptions.FIX;
			fixLoc.WLT_IsSystem = true;
			fixLoc.WLT_MaximumNumberOfProducts = 1;
			fixLoc.WLT_LocationClass = LocationClasses.Codes.FIX;

			var dynamicLoc = Factory.New<WhsLocationType>();
			dynamicLoc.WLT_Code = LocationClasses.Codes.DPF;
			dynamicLoc.WLT_IsActive = true;
			dynamicLoc.WLT_Description = LocationClasses.Descriptions.DPF;
			dynamicLoc.WLT_IsSystem = true;
			dynamicLoc.WLT_MaximumNumberOfProducts = 1;
			dynamicLoc.WLT_LocationClass = LocationClasses.Codes.DPF;

			var location = Factory.New<WhsLocation>();
			location.WLV_WLT_LocationType = fixLoc.PK;
			location.WLV_WA_PickingArea = area.PK;

			AssertNoErrors("Precondition", area.WA_AreaTypeInfo);

			location.WLV_WLT_LocationType = dynamicLoc.PK;

			AssertNoErrors("Still No error", area.WA_AreaTypeInfo);
		}

		#endregion

		#region TestWA_AreaType_DynamicCannotBeChangedIfAssignedToProducts

		public void TestWA_AreaType_CannotChangeFromDynamic_IfProductsAssigned()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var paramsByWhsAndClient = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			Factory.Save();

			AssertNoErrors("Precondition", dynamicArea.WA_AreaTypeInfo);

			dynamicArea.WA_AreaType = AreaTypes.Codes.FreeStore;

			AssertHasError(dynamicArea.WA_AreaTypeInfo, string.Format("Cannot change area type from {0} to {1}, as dynamic products have already been assigned.", AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore));
		}

		public void TestWA_AreaType_CanChangeFromDynamic_IfNoProductsAssigned()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			Factory.Save();

			AssertNoErrors("Precondition", dynamicArea.WA_AreaTypeInfo);

			dynamicArea.WA_AreaType = AreaTypes.Codes.FreeStore;

			AssertNoErrors("Area type can be changed as no products assigned to dynamic area", dynamicArea.WA_AreaTypeInfo);
		}

		public void TestWA_AreaType_CanChangeFromDynamic_IfProductAssignedToOtherArea()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicLocation1 = data.Whs1.FindLocation("A-1");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;
			var dynamicLocation2 = data.Whs1.FindLocation("A-2");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var paramsByWhsAndClient = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea2.PK;

			Factory.Save();

			AssertNoErrors("Precondition", dynamicArea1.WA_AreaTypeInfo);

			dynamicArea1.WA_AreaType = AreaTypes.Codes.FreeStore;

			AssertNoErrors("Area type can be changed as no products assigned to dynamic area", dynamicArea1.WA_AreaTypeInfo);
		}

		public void TestWA_AreaType_CannotChangeFromDynamic_MultipleProductsAssigned()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var paramsByWhsAndClient1 = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			var paramsByWhsAndClient2 = transactionHelper.CreateProductParamsByWhsAndClient(data.Part2.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient2[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			Factory.Save();

			AssertNoErrors("Precondition", dynamicArea.WA_AreaTypeInfo);

			dynamicArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError(dynamicArea.WA_AreaTypeInfo, string.Format("Cannot change area type from {0} to {1}, as dynamic products have already been assigned.", AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore));

			paramsByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = null;
			dynamicArea.RunPreSaveValidation();

			AssertHasError(dynamicArea.WA_AreaTypeInfo, string.Format("Cannot change area type from {0} to {1}, as dynamic products have already been assigned.", AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore));

			paramsByWhsAndClient2[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = null;
			dynamicArea.RunPreSaveValidation();

			AssertNoErrors("Area type can be changed as no products assigned to dynamic area", dynamicArea.WA_AreaTypeInfo);
		}

		public void TestWA_AreaType_CannotChangeFromDynamic_DuplicatedProductsAssigned()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("CL2");
			var org3 = Helper.CreateClient("CL3");
			var part2 = Helper.CreateProduct("P2", org3);

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var paramsByWhsAndClient1 = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, data.Org1.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			var paramsByWhsAndClient2 = transactionHelper.CreateProductParamsByWhsAndClient(data.Part1.PK, org2.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient2[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			var paramsByWhsAndClient3 = transactionHelper.CreateProductParamsByWhsAndClient(part2.PK, org3.PK, data.Whs1.PK, 2m, 10m, 3m, "CNT", 0);
			paramsByWhsAndClient2[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = dynamicArea.PK;

			Factory.Save();

			AssertNoErrors("Precondition", dynamicArea.WA_AreaTypeInfo);

			dynamicArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError(dynamicArea.WA_AreaTypeInfo, string.Format("Cannot change area type from {0} to {1}, as dynamic products have already been assigned.", AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore));

			paramsByWhsAndClient1[WhsProductParamsByWhsAndClientSchema.W3_WA_DynamicPickFaceArea] = null;
			dynamicArea.RunPreSaveValidation();

			AssertHasError(dynamicArea.WA_AreaTypeInfo, string.Format("Cannot change area type from {0} to {1}, as dynamic products have already been assigned.", AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore));
		}

		#endregion

		#region TestCheckWA_AreaType_InwardProcessingAreas_IsVirtualWarehouse

		public void TestCheckWA_AreaType_InwardProcessingAreas_IsVirtualWarehouse()
		{
			var warehouse = Helper.CreateWarehouse("WHS9");
			var inwardProcessingArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);

			const string errorMessage = "Inward Processing Areas must only be used in Virtual Warehouses.";
			AssertHasError(inwardProcessingArea.WA_AreaTypeInfo, errorMessage);

			inwardProcessingArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertNoErrorContaining(inwardProcessingArea.WA_AreaTypeInfo, errorMessage);

			inwardProcessingArea.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertHasError(inwardProcessingArea.WA_AreaTypeInfo, errorMessage);

			warehouse.WW_IsVirtualWarehouse = true;
			inwardProcessingArea.Validation.ValidateWA_AreaType();
			AssertNoErrorContaining(inwardProcessingArea.WA_AreaTypeInfo, errorMessage);

			var warehouseLessArea = Factory.New<WhsArea>();
			warehouseLessArea.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertHasError(warehouseLessArea.WA_AreaTypeInfo, errorMessage);
		}

		#endregion

		#region TestCheckWA_AreaType_CannotChangeBondedToNonBonded_IfAreaHasSOH

		public void TestCheckWA_AreaType_CannotChangeBondedToNonBonded_IfAreaHasSOH_PutawayArea()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "Bonded", AreaTypes.Codes.Bonded);
			var normalLocationType = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = normalLocationType.PK;
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WLT_LocationType = normalLocationType.PK;
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.Bonded, bondedArea.WA_AreaType);

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", "CUS", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-2", "A-2");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			var bondedSOHError = "Area Type cannot be changed from Bonded Area Type when the Area already contains inventory.";
			AssertNoErrorContaining(bondedArea.WA_AreaTypeInfo, bondedSOHError);

			bondedArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError("Should not allow changing bonded areas with stock to non-bonded", bondedArea.WA_AreaTypeInfo, bondedSOHError);

			bondedArea.WA_AreaType = AreaTypes.Codes.Bonded;
			AssertNoErrorContaining(bondedArea.WA_AreaTypeInfo, bondedSOHError);
		}

		public void TestCheckWA_AreaType_CannotChangeBondedToNonBonded_IfAreaHasSOH_PickingArea()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var bondedPutawayArea = Helper.CreateArea(data.Whs1, "Bonded1", AreaTypes.Codes.Bonded);
			var bondedPickingArea = Helper.CreateArea(data.Whs1, "Bonded2", AreaTypes.Codes.Bonded);
			var normalLocationType = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = normalLocationType.PK;
			location1.WLV_WA_PutawayArea = bondedPutawayArea.PK;
			location1.WLV_WA_PickingArea = bondedPickingArea.PK;
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WLT_LocationType = normalLocationType.PK;
			location2.WLV_WA_PutawayArea = bondedPutawayArea.PK;
			location2.WLV_WA_PickingArea = bondedPickingArea.PK;
			Factory.Save();

			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.Bonded, bondedPutawayArea.WA_AreaType);
			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.Bonded, bondedPickingArea.WA_AreaType);

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", "CUS", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-2", "A-2");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			var bondedSOHError = "Area Type cannot be changed from Bonded Area Type when the Area already contains inventory.";
			AssertNoErrorContaining(bondedPickingArea.WA_AreaTypeInfo, bondedSOHError);

			bondedPickingArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError("Should not allow changing bonded areas with stock to non-bonded", bondedPickingArea.WA_AreaTypeInfo, bondedSOHError);

			bondedPickingArea.WA_AreaType = AreaTypes.Codes.Bonded;
			AssertNoErrorContaining(bondedPickingArea.WA_AreaTypeInfo, bondedSOHError);
		}

		#endregion

		#region TestCheckWA_AreaType_CannotChangeBondedToNonBonded_IfAreaHasTransitPackages

		public void TestCheckWA_AreaType_CannotChangeBondedToNonBonded_IfAreaHasTransitPackages_PutawayArea()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS", "A", 5, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);

			var bondedArea = Helper.CreateArea(warehouse, "Bonded", AreaTypes.Codes.Bonded);
			Factory.Save();

			var dockDoor = warehouse.FindLocation("DOCK");
			var location1 = warehouse.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = bondedArea.PK;
			var location2 = warehouse.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.Bonded, bondedArea.WA_AreaType);

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, dockDoor.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = dockDoor.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package1 = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location1.PK;
			packageState[WhsItemPackageStateSchema.WPS_Status] = "ARV";
			packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package1.PK;
			packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = warehouse.PK;
			packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			Factory.Save();

			var bondedError = "Area Type cannot be changed from Bonded Area Type when the Area already contains packages.";
			AssertNoErrorContaining(bondedArea.WA_AreaTypeInfo, bondedError);

			bondedArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError("Should not allow changing bonded areas with stock to non-bonded", bondedArea.WA_AreaTypeInfo, bondedError);

			bondedArea.WA_AreaType = AreaTypes.Codes.Bonded;
			AssertNoErrorContaining(bondedArea.WA_AreaTypeInfo, bondedError);
		}

		public void TestCheckWA_AreaType_CannotChangeBondedToNonBonded_IfAreaHasTransitPackages_PickingArea()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS", "A", 5, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			warehouse.Rows.Add(row);

			var bondedArea = Helper.CreateArea(warehouse, "Bonded", AreaTypes.Codes.Bonded);
			Factory.Save();

			var dockDoor = warehouse.FindLocation("DOCK");
			var location1 = warehouse.FindLocation("A-1");
			location1.WLV_WA_PickingArea = bondedArea.PK;
			var location2 = warehouse.FindLocation("A-2");
			location2.WLV_WA_PickingArea = bondedArea.PK;
			Factory.Save();

			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.Bonded, bondedArea.WA_AreaType);

			var rtu = (BusinessObject)Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, dockDoor.PK);
			rtu[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = dockDoor.PK;

			var packageJob = Helper.CreatePackageJob(rtu);
			var package1 = (BusinessObject)Helper.CreatePackage(packageJob);
			var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location1.PK;
			packageState[WhsItemPackageStateSchema.WPS_Status] = "ARV";
			packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package1.PK;
			packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = warehouse.PK;
			packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			Factory.Save();

			var bondedError = "Area Type cannot be changed from Bonded Area Type when the Area already contains packages.";
			AssertNoErrorContaining(bondedArea.WA_AreaTypeInfo, bondedError);

			bondedArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError("Should not allow changing bonded areas with stock to non-bonded", bondedArea.WA_AreaTypeInfo, bondedError);

			bondedArea.WA_AreaType = AreaTypes.Codes.Bonded;
			AssertNoErrorContaining(bondedArea.WA_AreaTypeInfo, bondedError);
		}

		#endregion

		#region TestCheckWA_AreaType_CannotChangeInwardProcessingAreaToNonInwardProcessing

		public void TestCheckWA_AreaType_CannotChangeInwardProcessingAreaToNonInwardProcessing()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var iprArea1 = Helper.CreateArea(data.Whs1, "IP1", AreaTypes.Codes.InwardProcessing);
			var iprArea2 = Helper.CreateArea(data.Whs1, "IP2", AreaTypes.Codes.InwardProcessing);
			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.InwardProcessing, iprArea1.WA_AreaType);
			AssertEquals("Precondition: Area type is correct", AreaTypes.Codes.InwardProcessing, iprArea2.WA_AreaType);

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			// Hack, no other way to get inventory into an IPR area yet.
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = iprArea1.PK;
			location1.WLV_WA_PickingArea = iprArea1.PK;

			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = iprArea2.PK;
			location2.WLV_WA_PickingArea = iprArea2.PK;
			Factory.Save();

			const string iprError = "Area Type cannot be changed from Inward Processing Area Type.";
			AssertNoErrorContaining(iprArea1.WA_AreaTypeInfo, iprError);
			AssertNoErrorContaining(iprArea2.WA_AreaTypeInfo, iprError);

			iprArea1.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError(iprArea1.WA_AreaTypeInfo, iprError);

			iprArea2.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertHasError(iprArea2.WA_AreaTypeInfo, iprError);

			iprArea1.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertNoErrors(iprArea1.WA_AreaTypeInfo);

			iprArea2.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertNoErrors(iprArea2.WA_AreaTypeInfo);
		}

		#endregion

		#region TestCheckWA_AreaType_CannotChangeNonInwardProcessingAreaToInwardProcessing

		public void TestCheckWA_AreaType_CannotChangeNonInwardProcessingAreaToInwardProcessing()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area1 = Helper.CreateArea(data.Whs1, "FR1", AreaTypes.Codes.FreeStore);
			var area2 = Helper.CreateArea(data.Whs1, "FR2", AreaTypes.Codes.FreeStore);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PutawayArea = area1.PK;
			location1.WLV_WA_PickingArea = area1.PK;

			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = area2.PK;
			location2.WLV_WA_PickingArea = area2.PK;
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, ZDate.Empty, ZDate.Empty, "", "", "", "EntryKey-1", "A-1");
			transactionTestHelper.FinaliseDocket(receivePK);
			Factory.Save();

			const string iprError = "Area Type cannot be changed to Inward Processing Area Type.";
			AssertNoErrorContaining(area1.WA_AreaTypeInfo, iprError);
			AssertNoErrorContaining(area2.WA_AreaTypeInfo, iprError);

			area1.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertHasError(area1.WA_AreaTypeInfo, iprError);

			area2.WA_AreaType = AreaTypes.Codes.InwardProcessing;
			AssertHasError(area2.WA_AreaTypeInfo, iprError);

			area1.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertNoErrors(area1.WA_AreaTypeInfo);

			area2.WA_AreaType = AreaTypes.Codes.FreeStore;
			AssertNoErrors(area2.WA_AreaTypeInfo);
		}

		#endregion

		#region TestCheckWA_AreaType_VATFiscal

		public void TestCheckWA_AreaType_VATFiscal()
		{
			const string message = "VAT Fiscal Areas must only be used in EU Warehouses.";
			var lvAddress = Factory.New<OrgAddress>();
			lvAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Latvia;
			var gfAddress = Factory.New<OrgAddress>();
			gfAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.FrenchGuyana;
			var trAddress = Factory.New<OrgAddress>();
			trAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Turkey;
			var auAddress = Factory.New<OrgAddress>();
			auAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;

			var warehouse = Helper.CreateWarehouse("WHS");
			var area = Helper.CreateArea(warehouse, "Area1");

			warehouse.WW_OA_WarehouseAddress = lvAddress.PK;
			area.WA_AreaType = AreaTypes.Codes.VATFiscal;
			AssertNoError("European Union member states", area.WA_AreaTypeInfo, message);

			warehouse.WW_OA_WarehouseAddress = gfAddress.PK;
			area.Validation.ValidateWA_AreaType();
			AssertNoError("Jurisdiction of FR", area.WA_AreaTypeInfo, message);

			warehouse.WW_OA_WarehouseAddress = trAddress.PK;
			area.Validation.ValidateWA_AreaType();
			AssertNoError("Inherits from EU", area.WA_AreaTypeInfo, message);

			warehouse.WW_OA_WarehouseAddress = auAddress.PK;
			area.Validation.ValidateWA_AreaType();
			AssertHasError("Non-European", area.WA_AreaTypeInfo, message);

			area.WA_AreaType = AreaTypes.Codes.Bonded;
			AssertNoError("Not VAT type", area.WA_AreaTypeInfo, message);
		}

		#endregion

		#endregion

		#region TestCheckIfClientAndDischargeLRCCombinationIsValid

		public void TestCheckIfClientAndDischargeLRCCombinationIsValid()
		{
			var warehouse = CreateWarehouse();
			var clielt1 = Helper.CreateClient("C1");
			var clielt2 = Helper.CreateClient("C2");
			var zone1 = CreateRefZoneHeader("TST1", "TST1 Zone", ZoneTypeCodeDescriptionPair.TransitWarehouse.Code);
			var zone2 = CreateRefZoneHeader("TST2", "TST2 Zone", ZoneTypeCodeDescriptionPair.TransitWarehouse.Code);
			var area1 = warehouse.Areas[0];
			var area2 = warehouse.Areas[1];

			SetTransitClientAndDischargeLRC(area1, clielt1, zone1);
			SetTransitClientAndDischargeLRC(area2, clielt1, zone1);
			SetTransitClientAndDischargeLRC(area2, clielt1, null); // is valid as not using same zone
			AssertNoErrors("Should have no error.", area2.WA_OH_TransitClientInfo);

			SetTransitClientAndDischargeLRC(area2, null, zone1); // is valid as different client can use same zone
			AssertNoErrors("Should have no error.", area2.WA_OH_TransitClientInfo);

			SetTransitClientAndDischargeLRC(area2, clielt1, zone2); // is valid as not using another zone
			AssertNoErrors("Should have no error.", area2.WA_OH_TransitClientInfo);

			SetTransitClientAndDischargeLRC(area2, clielt2, zone1); // is valid as different clients using same zone
			AssertNoErrors("Should have no error.", area2.WA_OH_TransitClientInfo);

			SetTransitClientAndDischargeLRC(area1, null, zone1);
			SetTransitClientAndDischargeLRC(area2, null, null); // is valid as different clients using same zone
			AssertNoErrors("Should have no error.", area2.WA_OH_TransitClientInfo);
		}

		void SetTransitClientAndDischargeLRC(WhsArea area, OrgHeader client, RefZoneHeader zone)
		{
			area.WA_OH_TransitClient = client != null ? client.PK : ZGuid.Empty;
		}

		WhsWarehouse CreateWarehouse()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = warehouse.Areas.AddNew();
			area.WA_Name = "A2";
			area.WA_WW_Whs = warehouse.PK;

			return warehouse;
		}

		RefZoneHeader CreateRefZoneHeader(string code, string description, string type)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = code;
			zone.FZ_Description = description;
			zone.FZ_ZoneType = type;

			return zone;
		}

		#endregion

		#region TestCheckWA_CalcMaxWeight

		public void TestCheckWA_CalcMaxWeight()
		{
			var area = SetupAreaForCalculationValidationTest();
			var poke1 = area.WA_CalcMaxWeight;
			AssertNoWarnings("Precondition", area.WA_CalcMaxWeightInfo);

			area.Validation.ValidateWA_CalcMaxWeight();
			AssertHasWarning(area.WA_CalcMaxWeightInfo, "Cannot calculate Max Weight because the following Location(s) have invalid units:\r\n\tLocation 'A-1-1' has an invalid Weight Unit 'L'.");

			var location = area.Warehouse.FindLocation("A-1-1");
			Helper.SetLocationMaxWeightAndVolume(location, 10m, "KG", 1m, "M3"); // valid units
			var poke2 = area.WA_CalcMaxWeight;
			area.Validation.ValidateWA_CalcMaxWeight();
			AssertNoWarnings(area.WA_CalcMaxWeightInfo);
		}

		#endregion

		#region TestCheckWA_CalcMaxVolume

		public void TestCheckWA_CalcMaxVolume()
		{
			var area = SetupAreaForCalculationValidationTest();
			var poke1 = area.WA_CalcMaxVolume;
			AssertNoWarnings("Precondition", area.WA_CalcMaxVolumeInfo);

			area.Validation.ValidateWA_CalcMaxVolume();
			AssertHasWarning(area.WA_CalcMaxVolumeInfo, "Cannot calculate Max Volume because the following Location(s) have invalid units:\r\n\tLocation 'A-1-1' has an invalid Volume Unit 'CM'.");

			var location = area.Warehouse.FindLocation("A-1-1");
			Helper.SetLocationMaxWeightAndVolume(location, 10m, "KG", 1m, "M3"); // valid units
			var poke2 = area.WA_CalcMaxVolume;
			area.Validation.ValidateWA_CalcMaxVolume();
			AssertNoWarnings(area.WA_CalcMaxVolumeInfo);
		}

		#endregion

		#region TestCheckWA_CalcCurrentWeight

		public void TestCheckWA_CalcCurrentWeight()
		{
			var area = SetupAreaForCalculationValidationTest();
			var poke = area.WA_CalcCurrentWeight;
			AssertNoWarnings("Precondition", area.WA_CalcCurrentWeightInfo);

			area.Validation.ValidateWA_CalcCurrentWeight();
			AssertHasWarning(area.WA_CalcCurrentWeightInfo, "Cannot calculate Weight because the following Product(s) have invalid units:\r\n\tProduct 'P1' has an invalid Weight Unit 'L'.");
		}

		#endregion

		#region TestCheckWA_CalcCurrentVolume

		public void TestCheckWA_CalcCurrentVolume()
		{
			var area = SetupAreaForCalculationValidationTest();
			var poke = area.WA_CalcCurrentVolume;
			AssertNoWarnings("Precondition", area.WA_CalcCurrentVolumeInfo);

			area.Validation.ValidateWA_CalcCurrentVolume();
			AssertHasWarning(area.WA_CalcCurrentVolumeInfo, "Cannot calculate Volume because the following Product(s) have invalid units:\r\n\tProduct 'P1' has an invalid Volume Unit 'CM'.");
		}

		#endregion

		#region TestCheckRFPickPackPrinterPK

		public void TestCheckRFPickPackPrinterPK()
		{
			var area = Factory.New<WhsArea>();
			AssertNoErrors("Precondition", area.RFPickPackPrinterPKInfo);

			area.RFPickPackPrinterPK = ZGuid.Invalid;
			AssertHasError(area.RFPickPackPrinterPKInfo, "Enter a valid RF Pick Pack Printer.");

			area.RFPickPackPrinterPK = ZGuid.Empty;
			AssertNoErrors(area.RFPickPackPrinterPKInfo);
		}

		#endregion

		#region TestCheckWA_IsPutawayAreaAndPickingAreaAtLeastOneIsSelected

		public void TestCheckWA_IsPutawayAreaAndPickingAreaAtLeastOneIsSelected()
		{
			var area = Factory.New<WhsArea>();
			area.WA_IsPutawayArea = false;
			area.WA_IsPickingArea = false;
			AssertHasError("Since both flags are turned off, WA_IsPickingAreaInfo must have errors.", area.WA_IsPickingAreaInfo, "Area must at least be picking or putaway area.");

			area.WA_IsPutawayArea = true;
			area.WA_IsPutawayArea = false; // trigger the validation
			AssertHasError("Since both flags are turned off, WA_IsPutawayAreaInfo must have errors.", area.WA_IsPutawayAreaInfo, "Area must at least be picking or putaway area.");

			area.WA_IsPickingArea = true;
			AssertNoErrors("Since WA_IsPickingArea is enabled, WA_IsPutawayAreaInfo must not have any errors.", area.WA_IsPickingAreaInfo);

			area.WA_IsPickingArea = false;
			area.WA_IsPutawayArea = true;
			AssertNoErrors("Since WA_IsPutawayArea is enabled, WA_IsPutawayAreaInfo must not have any errors.", area.WA_IsPutawayAreaInfo);

			area.WA_IsPickingArea = true;
			AssertNoErrors("Since both WA_IsPutawayArea and WA_IsPickingArea is enabled, WA_IsPutawayAreaInfo must not have any errors.", area.WA_IsPutawayAreaInfo);
		}

		#endregion

		#region TestCheckWA_IsPickingAreaCannotBeDisabledIfALocationIsReferencingIt

		public void TestCheckWA_IsPickingAreaCannotBeDisabledIfALocationIsReferencingIt()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			var areaUsedAsBothPickingAndPutAway = Helper.CreateArea(warehouse, "A1");
			var areaUsedAsPickingArea = Helper.CreateArea(warehouse, "A2");
			var areaUsedAsPutawayArea = Helper.CreateArea(warehouse, "A3");
			var areaNotUsedAnyLocations = Helper.CreateArea(warehouse, "A4");

			Helper.CreateRowAndGenerateLocations(warehouse, "L1");
			Helper.CreateRowAndGenerateLocations(warehouse, "L2");
			Helper.CreateRowAndGenerateLocations(warehouse, "L3");
			Factory.Save();

			var location1 = warehouse.FindLocation("L1");
			var location2 = warehouse.FindLocation("L2");
			var location3 = warehouse.FindLocation("L3");

			SetPickingAndPutAwayArea(location1, areaUsedAsBothPickingAndPutAway.PK, areaUsedAsBothPickingAndPutAway.PK);
			SetPickingAndPutAwayArea(location2, areaUsedAsPickingArea.PK, areaUsedAsBothPickingAndPutAway.PK);
			SetPickingAndPutAwayArea(location3, areaUsedAsBothPickingAndPutAway.PK, areaUsedAsPutawayArea.PK);

			areaUsedAsBothPickingAndPutAway.WA_IsPickingArea = false;
			AssertHasError(areaUsedAsBothPickingAndPutAway.WA_IsPickingAreaInfo, "Area is used as a picking area in location(s) hence cannot change it to a non-pick area.");

			areaUsedAsPickingArea.WA_IsPickingArea = false;
			AssertHasError(areaUsedAsPickingArea.WA_IsPickingAreaInfo, "Area is used as a picking area in location(s) hence cannot change it to a non-pick area.");

			areaUsedAsPutawayArea.WA_IsPickingArea = false;
			AssertNoErrors(areaUsedAsPutawayArea.WA_IsPickingAreaInfo);

			areaNotUsedAnyLocations.WA_IsPickingArea = false;
			AssertNoErrors(areaNotUsedAnyLocations.WA_IsPickingAreaInfo);
		}

		#endregion

		#region TestCheckWA_IsPutawayAreaCannotBeDisabledIfALocationIsReferencingIt

		public void TestCheckWA_IsPutawayAreaCannotBeDisabledIfALocationIsReferencingIt()
		{
			var warehouse = Helper.CreateWarehouse("W1");
			var areaUsedAsBothPickingAndPutAway = Helper.CreateArea(warehouse, "A1");
			var areaUsedAsPickingArea = Helper.CreateArea(warehouse, "A2");
			var areaUsedAsPutawayArea = Helper.CreateArea(warehouse, "A3");
			var areaNotUsedAnyLocations = Helper.CreateArea(warehouse, "A4");

			Helper.CreateRowAndGenerateLocations(warehouse, "L1");
			Helper.CreateRowAndGenerateLocations(warehouse, "L2");
			Helper.CreateRowAndGenerateLocations(warehouse, "L3");
			Factory.Save();

			var location1 = warehouse.FindLocation("L1");
			var location2 = warehouse.FindLocation("L2");
			var location3 = warehouse.FindLocation("L3");

			SetPickingAndPutAwayArea(location1, areaUsedAsBothPickingAndPutAway.PK, areaUsedAsBothPickingAndPutAway.PK);
			SetPickingAndPutAwayArea(location2, areaUsedAsPickingArea.PK, areaUsedAsBothPickingAndPutAway.PK);
			SetPickingAndPutAwayArea(location3, areaUsedAsBothPickingAndPutAway.PK, areaUsedAsPutawayArea.PK);

			areaUsedAsBothPickingAndPutAway.WA_IsPutawayArea = false;
			AssertHasError(areaUsedAsBothPickingAndPutAway.WA_IsPutawayAreaInfo, "Area is used as a putaway area in location(s) hence cannot change it to a non-putaway area.");

			areaUsedAsPutawayArea.WA_IsPutawayArea = false;
			AssertHasError(areaUsedAsPutawayArea.WA_IsPutawayAreaInfo, "Area is used as a putaway area in location(s) hence cannot change it to a non-putaway area.");

			areaUsedAsPickingArea.WA_IsPutawayArea = false;
			AssertNoErrors(areaUsedAsPickingArea.WA_IsPutawayAreaInfo);

			areaNotUsedAnyLocations.WA_IsPutawayArea = false;
			AssertNoErrors(areaNotUsedAnyLocations.WA_IsPutawayAreaInfo);
		}

		#endregion

		#region TestWA_IsPickingArea_DynamicPickFace

		public void TestWA_IsPickingArea_DynamicPickFace()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");

			var area = Factory.New<WhsArea>();
			area.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			area.WA_IsPickingArea = true;

			var locationType = Factory.New<WhsLocationType>();
			locationType.WLT_Code = "DPF";
			locationType.WLT_IsActive = true;
			locationType.WLT_Description = AreaTypes.Descriptions.DynamicPickFace;
			locationType.WLT_IsSystem = true;
			locationType.WLT_MaximumNumberOfProducts = 1;
			locationType.WLT_LocationClass = AreaTypes.Codes.DynamicPickFace;

			var location = Factory.New<WhsLocation>();
			location.WLV_WLT_LocationType = locationType.PK;
			location.WLV_WA_PickingArea = area.PK;

			AssertNoErrors("Precondition: No errors.", area.WA_IsPickingAreaInfo);

			area.WA_IsPickingArea = false;

			AssertHasError(area.WA_IsPickingAreaInfo, "Dynamic Pick Face Areas must be a picking area.");
		}

		#endregion

		#region TestWA_IsPutawayArea_DynamicPickFace

		public void TestWA_IsPutawayArea_DynamicPickFace()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");

			var area = Factory.New<WhsArea>();
			area.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			area.WA_IsPutawayArea = false;

			var locationType = Factory.New<WhsLocationType>();
			locationType.WLT_Code = "DPF";
			locationType.WLT_IsActive = true;
			locationType.WLT_Description = AreaTypes.Descriptions.DynamicPickFace;
			locationType.WLT_IsSystem = true;
			locationType.WLT_MaximumNumberOfProducts = 1;
			locationType.WLT_LocationClass = AreaTypes.Codes.DynamicPickFace;

			var location = Factory.New<WhsLocation>();
			location.WLV_WLT_LocationType = locationType.PK;
			location.WLV_WA_PickingArea = area.PK;

			AssertNoErrors("Precondition: No errors.", area.WA_IsPutawayAreaInfo);

			area.WA_IsPutawayArea = true;

			AssertHasError(area.WA_IsPutawayAreaInfo, "Dynamic Pick Face Areas cannot be a putaway area.");
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var area = SetupAreaForCalculationValidationTest();
			var poke1 = area.WA_CalcCurrentVolume;
			var poke2 = area.WA_CalcCurrentWeight;
			var poke3 = area.WA_CalcMaxWeight;
			var poke4 = area.WA_CalcMaxVolume;

			using (area.GetValidationSuspender())
			{
				area.RFPickPackPrinterPK = ZGuid.Invalid;
			}

			AssertNoWarnings("Precondition", area.WA_CalcCurrentWeightInfo);
			AssertNoWarnings("Precondition", area.WA_CalcCurrentVolumeInfo);
			AssertNoWarnings("Precondition", area.WA_CalcMaxWeightInfo);
			AssertNoWarnings("Precondition", area.WA_CalcMaxVolumeInfo);
			AssertNoErrors("Precondition", area.RFPickPackPrinterPKInfo);

			area.Validation.ValidateAll();
			AssertHasWarning(area.WA_CalcCurrentWeightInfo, "Cannot calculate Weight because the following Product(s) have invalid units:\r\n\tProduct 'P1' has an invalid Weight Unit 'L'.");
			AssertHasWarning(area.WA_CalcCurrentVolumeInfo, "Cannot calculate Volume because the following Product(s) have invalid units:\r\n\tProduct 'P1' has an invalid Volume Unit 'CM'.");
			AssertHasWarning(area.WA_CalcMaxWeightInfo, "Cannot calculate Max Weight because the following Location(s) have invalid units:\r\n\tLocation 'A-1-1' has an invalid Weight Unit 'L'.");
			AssertHasWarning(area.WA_CalcMaxVolumeInfo, "Cannot calculate Max Volume because the following Location(s) have invalid units:\r\n\tLocation 'A-1-1' has an invalid Volume Unit 'CM'.");
			AssertHasError(area.RFPickPackPrinterPKInfo, "Enter a valid RF Pick Pack Printer.");
		}

		#endregion

		#region Implementation

		WhsArea SetupAreaForCalculationValidationTest()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var area = warehouse.Areas[0];
			Factory.Save();
			var location = warehouse.FindLocation("A-1-1");
			Factory.Save();

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = (OrgSupplierPart)iHelper.CreateProduct(orgPK, "P1");
			part.OP_CubicUQ = "CM";
			part.OP_WeightUQ = "L";

			var receivePK = iHelper.CreateWhsReceive(orgPK, warehouse.PK, "1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-1");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1-2");
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-2-1");
			iHelper.FinaliseDocket(receivePK);
			Factory.Save(); // needed for DBOnly Query

			Env.Registry.PackageWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			Env.Registry.PackageVolumeUnit = Enterprise.Core.Constants.Volume.CubicMetres;
			Helper.SetLocationMaxWeightAndVolume(location, 10m, "L", 1m, "CM"); // invalid units, not valid to save to database so must be done last
			return area;
		}

		void SetPickingAndPutAwayArea(WhsLocation location, ZGuid pickingAreaPK, ZGuid putawayAreaPK)
		{
			location.WLV_WA_PickingArea = pickingAreaPK;
			location.WLV_WA_PutawayArea = putawayAreaPK;
		}

		public WhsArea Area
		{
			get { return area ?? (area = Factory.New<WhsArea>()); }
			set { area = value; }
		}

		WhsArea area;

		#endregion
	}
}
