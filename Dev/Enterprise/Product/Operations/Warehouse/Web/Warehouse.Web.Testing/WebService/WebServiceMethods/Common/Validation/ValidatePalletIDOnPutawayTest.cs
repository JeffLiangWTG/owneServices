using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ValidatePalletIDOnPutawayTest : ValidatePalletIDOnPutawaySharedTest
	{
		#region TestValidatePalletIDOnPutaway

		[TestDate(2009, 1, 1)]
		public void TestValidatePalletIDOnPutaway()
		{
			var year = ZDateTime.Today.Year - 2;

			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var data = new TestDataSimpleEnvironment(helper.Factory, 3, 1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			data.Org1.MiscServ.OM_IMUsePackingDate = true;
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			helper.Factory.Save();

			var receive_Other = helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive_Other.WD_ExternalReference = "12345";
			var inventory_Other = helper.CreateWhsReceiveInventoryLine(receive_Other, data.Part1, 10m, new ZDate(year, 04, 02),
				new ZDate(year, 04, 01), "P1A1", "P1A2", "P1A3", "");
			inventory_Other.WI_PalletID = "PalletID1";
			inventory_Other.WI_F3_NKPackType = "M3";
			helper.Factory.Save();

			var response1 = webService.ValidatePalletIDOnPutaway("", false);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("Provide Pallet ID.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var response2 = webService.ValidatePalletIDOnPutaway("PalletID0", false);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals("Pallet ID PalletID0 does not exist on an Un-Finalized Receipt.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var response3 = webService.ValidatePalletIDOnPutaway("PalletID1", false);
			AssertSuccessfulResponse(response3, webService);
			AssertEquals(null, response3.ErrorMessage);
			AssertEquals(ErrorTypes.None, response3.Error);
			AssertEquals(response3.PalletID, "PalletID1");
			AssertNotNull(response3.Inventory);
			AssertEquals(response3.Inventory.InventoryLineInfos.Count, 1);
			AssertNotNull(response3.Inventory.ProductInfos[0]);
			AssertEquals("P1", response3.Inventory.ProductInfos[0].Code);
			AssertEquals("DestinationLocation should not be set yet", "", response3.Inventory.InventoryLineInfos[0].DestinationLocation);
			AssertEquals("DestinationPalletId should putaway transfer's pallet id.", "PalletID1", response3.Inventory.InventoryLineInfos[0].DestinationPalletId);
			AssertEquals("Inventory location is default inbound DockDoor.", data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), response3.Inventory.InventoryLineInfos[0].Location);
			AssertEquals(new DateTime(year, 4, 2), response3.Inventory.InventoryLineInfos[0].ExpiryDate);
			AssertEquals(new DateTime(year, 4, 1), response3.Inventory.InventoryLineInfos[0].PackingDate);
			AssertEquals("P1A1", response3.Inventory.InventoryLineInfos[0].Attribute1);
			AssertEquals("P1A2", response3.Inventory.InventoryLineInfos[0].Attribute2);
			AssertEquals("P1A3", response3.Inventory.InventoryLineInfos[0].Attribute3);
			AssertEquals("Attr1", response3.Inventory.ProductPartAttributesInfos[0].Attribute1Caption);
			AssertEquals("Attr2", response3.Inventory.ProductPartAttributesInfos[0].Attribute2Caption);
			AssertEquals("Attr3", response3.Inventory.ProductPartAttributesInfos[0].Attribute3Caption);
			AssertEquals(10m, response3.Inventory.InventoryLineInfos[0].Packs);

			var transfer = ((WhsReceiveLine)inventory_Other.InDocketLine).PutawayTransfer;
			transfer.Lines[0].WE_WL = data.Whs1.DefaultLocation.PK;
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			helper.Factory.Save();

			receive_Other.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive_Other);
			helper.Factory.Save();

			var response4 = webService.ValidatePalletIDOnPutaway("PalletID1", false);
			AssertSuccessfulResponse(response4, webService);
			AssertEquals("Pallet ID PalletID1 does not exist on an Un-Finalized Receipt.", response4.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response4.Error);
		}

		public void TestValidatePalletIDOnPutaway_MultipleInventories_PalletIdOnArrivedAndPutawayInventory()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = data.Whs1.DefaultInboundDockDoorLocation.LocationType;

			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var defaultInboundDockDoor = data.Whs1.FindLocation("A-3");
			defaultInboundDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			data.Whs1.WW_DefaultInboundDockDoor = defaultInboundDockDoor.PK;

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location, "PLT1");
			inventory2.WI_ArrivalDate = DateTime.Now;
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, inventory2.WI_InventoryStatus);
			AssertNotEquals("Precondition: Inventory3 location is not default inbound DockDoor.", data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), location.ToLocationString());

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDOnPutaway("PLT1", false);

			AssertEquals("Putaway transfer creation failed because Pallet ID PLT1 must be entirely either Arrived, Received to Dock Door or Putaway.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("returned correct number of lines", 2, response.Inventory.InventoryLineInfos.Count);
		}

		public void TestValidatePalletIDOnPutaway_MultipleInventories_PalletIdOnPutawayAndNoLocationInventories()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "12345", 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = GetValidatePalletIDWebServiceResponse(webService1, "12345");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("No error in the response.", null, response1.ErrorMessage);

			AssertEquals("Precondition", true, inventory1.HasPutawayTransfer);

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW2", ZDateTimeOffset.Empty);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m, null, "12345");
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory2.WI_InventoryStatus);

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var response2 = GetValidatePalletIDWebServiceResponse(webService2, "12345");
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("No error in the response.", null, response2.ErrorMessage);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHits()
		{
			TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHits(expectedDockets: 21, expectedInvs: 31, () =>
			{
				return new Dictionary<string, int>()
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 2 }, // second hit coming from a fetch hint
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },
					{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 3 },
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickFaceSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 4 },
					{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsPutawayLineSchema.Constants.TableName, 1 },
				};
			});
		}

		[TestDate(2025, 6, 16)]
		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_TaskManagement_DBHits()
		{
			TestValidatePalletIDOnPutaway_OnePutawayTransfer_TaskManagement_DBHits(expectedDockets: 21, expectedInvs: 31, () =>
			{
				return new Dictionary<string, int>()
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 2 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 5 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 2 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },
					{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 3 },
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickFaceSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 4 },
					{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsPutawayLineSchema.Constants.TableName, 1 },
				};
			});
		}

		public void TestValidatePalletIDOnPutaway_DifferentPalletIdAfterAllocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT-1", 10m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine.WE_PalletID = "PLT-2";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var result = webService.ValidatePalletIDOnPutaway("PLT-1", false);
			AssertSuccessfulResponse(result, webService);
			AssertEquals(null, result.ErrorMessage);
			AssertEquals(ErrorTypes.None, result.Error);
			AssertEquals(result.PalletID, "PLT-1");
			AssertNotNull(result.Inventory);

			AssertEquals("Destination pallet id is the updated pallet id.", "PLT-2", result.Inventory.InventoryLineInfos[0].DestinationPalletId);
		}

		public void TestValidatePalletIDOnPutaway_PalletWithBondedStock()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var bondedArea = Helper.CreateArea(data.Whs1, "BONDED", AreaTypes.Codes.Bonded);
			var bondedDockDoor = data.Whs1.FindLocation("A-1");
			bondedDockDoor.WLV_WA_PutawayArea = bondedArea.PK;
			bondedDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, bondedDockDoor, "PLT1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PLT1", false);

			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ReferencesOfReceiveThatCouldBeAutoFinalised);
			AssertInventoryStatusAndEventCount(inventory, InventoryStatus.Codes.Received);

			var transfer = (WhsTransfer)inventory.AllPickLines.First().DocketLine.Docket;
			AssertPutawayTransfer(transfer, data.Org1, data.Whs1, 1);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_ShowStockOnHandWarningOnPutaway

		public void TestValidatePalletIDOnPutaway_ShowStockOnHandWarningOnPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService1 = GetNewWebService(data.Whs1);
				var response1 = webService1.ValidatePalletIDOnPutaway("PLT_1", false);
				AssertSuccessfulResponse(response1, webService1);
				AssertEquals("ShowStockOnHandWarningOnPutaway correct", true, response1.ShowStockOnHandWarningOnPutaway);
			}

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService2 = GetNewWebService(data.Whs1);
				var response2 = webService2.ValidatePalletIDOnPutaway("PLT_1", false);
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("ShowStockOnHandWarningOnPutaway correct", false, response2.ShowStockOnHandWarningOnPutaway);
			}
		}

		#endregion

		#region TestValidatePalletIDOnPutAway_InventoryReturnedHasCorrectStatusAndLocation

		public void TestValidatePalletIDOnPutAway_InventoryReturnedHasCorrectStatusAndLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);
			var webService = GetNewWebService(data.Whs1);
			var response = webService.ValidatePalletIDOnPutaway("PalletID1", false);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(response.PalletID, "PalletID1");
			AssertNotNull(response.Inventory);
			AssertEquals("There is 1 inventory line info returned.", 1, response.Inventory.InventoryLineInfos.Count);
			AssertEquals("Inventory status is received as a putaway transfer was created.", InventoryStatus.Codes.Received, response.Inventory.InventoryLineInfos[0].InventoryStatus);
			AssertEquals("Inventory location is DockDoor.", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), response.Inventory.InventoryLineInfos[0].Location);
		}

		public void TestValidatePalletIDOnPutaway_MultipleInventories_PalletIdOnReceivedAndArrivedInventories()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, null, "12345");
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory2.WI_InventoryStatus);

			var response = GetValidatePalletIDWebServiceResponse(GetNewWebService(data.Whs1, staff1), "12345");
			AssertEquals("Putaway transfer creation failed because Pallet ID 12345 must be entirely either Arrived, Received to Dock Door or Putaway.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestValidatePalletIDOnPutAway_InventoryReturnedHasCorrectStatusAndLocation_LocationIsAssigned()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PalletID1", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("No error in the response.", null, response1.ErrorMessage);

			var location = data.Whs1.FindLocation("A-1");
			AssertEquals("Precondition", true, inventory.HasPutawayTransfer);
			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			putawayTransferLine.WE_WL = location.PK;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response2 = webService.ValidatePalletIDOnPutaway("PalletID1", false);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals("No error in the response.", null, response2.ErrorMessage);
			AssertEquals("Inventory location has assigned location.", "A-1", response2.Inventory.InventoryLineInfos[0].DestinationLocation);
			AssertEquals("Inventory Pallet Id has assigned pallet id.", "PalletID1", response2.Inventory.InventoryLineInfos[0].DestinationPalletId);
		}

		public void TestValidatePalletIDOnPutAway_InventoryReturnedHasCorrectStatusAndLocation_MultipleInventorySamePalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1", 10m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition: Inventory status is Received.", InventoryStatus.Codes.Received, inventory3.WI_InventoryStatus);
			var webService = GetNewWebService(data.Whs1);
			var response = webService.ValidatePalletIDOnPutaway("PalletID1", false);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(response.PalletID, "PalletID1");
			AssertNotNull(response.Inventory);
			AssertEquals("There are 3 Receiveline inventory line infos returned.", 3, response.Inventory.InventoryLineInfos.Count);
			AssertEquals($"All Receiveline Inventory status' should be {InventoryStatus.Codes.Received} as a putaway transfer was created.", true,
				response.Inventory.InventoryLineInfos.All(inventory => inventory.InventoryStatus == InventoryStatus.Codes.Received));
			AssertEquals("All Receiveline Inventory locations should be DockDoor as putaway transfer will have putaway location.", true,
				response.Inventory.InventoryLineInfos.All(inventory => inventory.Location == data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString()));
		}

		public void TestValidatePalletIDOnPutaway_PalletIdOnArrivedInventory()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var defaultInboundDockDoor = data.Whs1.FindLocation("A-3");
			defaultInboundDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			data.Whs1.WW_DefaultInboundDockDoor = defaultInboundDockDoor.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, null, "PLT1");
			inventory.InDocketLine.WE_AdjustmentArrivalDate = DateTime.Now;
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, inventory.WI_InventoryStatus);

			var response = GetNewWebService(data.Whs1, staff1).ValidatePalletIDOnPutaway("PLT1", false);
			AssertEquals(null, response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals("returned correct number of lines", 1, response.Inventory.InventoryLineInfos.Count);
			AssertEquals("DestinationLocation should not be set yet", "", response.Inventory.InventoryLineInfos[0].DestinationLocation);
			AssertEquals("DestinationPalletId putaway transfer's pallet id.", "PLT1", response.Inventory.InventoryLineInfos[0].DestinationPalletId);
			AssertEquals("Inventory location is default inbound DockDoor.", data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), response.Inventory.InventoryLineInfos[0].Location);
			AssertEquals("Status is PFU", inventory.InDocketLine.WE_DocketLineStatus, "PFU");
		}

		public void TestValidatePalletIDOnPutaway_InventoryWithPutawayLocation()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var defaultInboundDockDoor = data.Whs1.FindLocation("A-3");
			defaultInboundDockDoor.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();
			data.Whs1.WW_DefaultInboundDockDoor = defaultInboundDockDoor.PK;

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location, "PLT1");
			inventory.WI_ArrivalDate = DateTime.Now;
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, inventory.WI_InventoryStatus);
			AssertNotEquals("Precondition: Inventory location is not default inbound DockDoor.", data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), location.ToLocationString());

			var response = GetNewWebService(data.Whs1, staff1).ValidatePalletIDOnPutaway("PLT1", false);
			AssertEquals(null, response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertEquals("returned correct number of lines", 1, response.Inventory.InventoryLineInfos.Count);
			AssertEquals("DestinationLocation should be set to location", location.ToLocationString(), response.Inventory.InventoryLineInfos[0].DestinationLocation);
			AssertEquals("DestinationPalletId should be set to pallet id to putaway", "PLT1", response.Inventory.InventoryLineInfos[0].DestinationPalletId);
			AssertEquals("Inventory location is default inbound DockDoor.", data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), response.Inventory.InventoryLineInfos[0].Location);
			AssertEquals("Status is PFU", inventory.InDocketLine.WE_DocketLineStatus, "PFU");
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_AllowToOverrideLocation

		public void TestValidatePalletIDOnPutaway_AllowToOverrideLocation_NoLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_3");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_4");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("Should allow to override location since no location were set to begin with.", true, response1.AllowToOverrideLocation);
		}

		public void TestValidatePalletIDOnPutaway_AllowToOverrideLocation_Unfinalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_3");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_4");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_3");

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponse(response, webService);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_2", false);
			AssertEquals("Should allow to override location since neither Receive1 nor Receive2 are finalised.", true, response.AllowToOverrideLocation);
		}

		public void TestValidatePalletIDOnPutaway_AllowToOverrideLocation_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_3");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_4");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_3");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, locations[1], "PLT_4");
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m, locations[0], "PLT_3");
			receive3.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive3);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PLT_3", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("Shouldn't allow to override location since Receive3 is finalised.", false, response1.AllowToOverrideLocation);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locations[1].ToLocationString(), "PLT_4", locations[0].ToLocationString(), "");
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.ValidatePalletIDOnPutaway("PLT_4", false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Shouldn't allow to override location since Receive3 is finalised.", true, response2.AllowToOverrideLocation);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_WithCancelledPalletIDs

		public void TestValidatePalletIDOnPutaway_WithCancelledPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_3");
			Helper.Factory.Save();

			var cancelledReceive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R3");
			var cancelledReceiveLine = Helper.CreateWhsReceiveLine(cancelledReceive, data.Part1, 5m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT_3");
			cancelledReceive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_StockOnHand = 0;
			Helper.Factory.Save();
			AssertEquals(DocketStatus.Codes.Cancelled, cancelledReceive.WD_DocketStatus);
			AssertEquals(DocketLineStatus.Codes.Cancelled, cancelledReceiveLine.WE_DocketLineStatus);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDOnPutaway("PLT_3", false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("No error in the response.", null, response1.ErrorMessage);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_ReassignPalletID

		public void TestValidatePalletIDOnPutaway_ReassignPalletID_QuestionAsked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2", 20m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			var putawayLine = Helper.CreateWhsPutawayLine(putawayJob, "PLT_2");
			Helper.Factory.Save();

			AssertEquals("WPL_IsPuttingAway correct", false, putawayLine.WPL_IsPuttingAway);
			var webService = GetNewWebService(data.Whs1, staff1);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals(ErrorTypes.None, response1.Error);

			webService = GetNewWebService(data.Whs1, staff1);
			var response2 = webService.ValidatePalletIDOnPutaway("PLT_2", false);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals(ErrorTypes.YesNoEnquiry, response2.Error);
			AssertEquals($"Entered Pallet ID(s) 'PLT_2' are already assigned to another user. Do you want to reassign the ID(s) to your Putaway Job?", response2.ErrorMessage);
			Helper.Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJob1 = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory, data.Whs1, staff1);
			AssertNull(putawayJob1);
		}

		public void TestValidatePalletIDOnPutaway_ReassignPalletID_YesResponse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_8");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_1", 10m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff2.GS_Code;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_8", 10m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff2.GS_Code;
			Helper.Factory.Save();
			AssertEquals("Putaway TransferLine1 WE_GS_PutawayBy correct.", staff2.GS_Code, transferLine1.WE_GS_NKPutawayBy);
			AssertEquals("Putaway TransferLine2 WE_GS_PutawayBy correct.", staff2.GS_Code, transferLine2.WE_GS_NKPutawayBy);

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_1");
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_8");
			Helper.Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJob1 = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory, data.Whs1, staff2);
			AssertNotNull(putawayJob1);
			var putawayLines1 = putawayJob1.Lines;
			AssertEquals("PutawayJob Lines count correct.", 2, putawayLines1.Count);
			AssertEquals("WPL_PalletID correct", "PLT_1", putawayLines1[0].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", false, putawayLines1[0].WPL_IsPuttingAway);
			AssertEquals("WPL_PalletID correct", "PLT_8", putawayLines1[1].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", false, putawayLines1[1].WPL_IsPuttingAway);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_1", true);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals(ErrorTypes.None, response1.Error);

			webService = GetNewWebService(data.Whs1, staff1);
			var response2 = webService.ValidatePalletIDOnPutaway("PLT_8", true);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals(ErrorTypes.None, response2.Error);

			var putawayJob1After = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory, data.Whs1, staff1);
			AssertNull(putawayJob1After);

			var otherFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJob2After = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory2, data.Whs1, staff2);
			AssertNotNull(putawayJob2After);
			var putawayLines2After = putawayJob2After.Lines;
			AssertEquals("PutawayJob Lines count correct.", 0, putawayLines2After.Count);
		}

		public void TestValidatePalletIDOnPutaway_PalletIDOnDifferentJobIsPuttingAway_NotReassigning()
		{
			TestValidatePalletIDOnPutaway_PalletIDOnDifferentJobIsPuttingAwayCore(isReassigning: false);
		}

		public void TestValidatePalletIDOnPutaway_PalletIDOnDifferentJobIsPuttingAway_IsReassigning()
		{
			TestValidatePalletIDOnPutaway_PalletIDOnDifferentJobIsPuttingAwayCore(isReassigning: true);
		}

		public void TestValidatePalletIDOnPutaway_PalletIDOnDifferentJobIsPuttingAwayCore(bool isReassigning)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2", 20m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			var putawayLine = Helper.CreateWhsPutawayLine(putawayJob, "PLT_2", true);
			Helper.Factory.Save();

			AssertEquals(true, putawayLine.WPL_IsPuttingAway);
			var webService = GetNewWebService(data.Whs1, staff1);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_1", isReassigning);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals(ErrorTypes.None, response1.Error);

			webService = GetNewWebService(data.Whs1, staff1);
			var response2 = webService.ValidatePalletIDOnPutaway("PLT_2", isReassigning);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals("Entered Pallet ID(s) 'PLT_2' are already assigned to another user and are currently being put away.", response2.ErrorMessage);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_PutawayJobExsitedWithSameUser

		public void TestValidatePalletIDOnPutaway_PutawayJobExistedWithSameUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_1", 10m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff.GS_Code;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_2", 10m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff.GS_Code;
			Helper.Factory.Save();
			AssertEquals("Putaway TransferLine1 WE_GS_PutawayBy correct.", staff.GS_Code, transferLine1.WE_GS_NKPutawayBy);
			AssertEquals("Putaway TransferLine2 WE_GS_PutawayBy correct.", staff.GS_Code, transferLine2.WE_GS_NKPutawayBy);

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_1");
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2");
			Helper.Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJob1 = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory, data.Whs1, staff);
			AssertNotNull(putawayJob1);
			var putawayLines1 = putawayJob1.Lines;
			AssertEquals("PutawayJob Lines count correct.", 2, putawayLines1.Count);
			AssertEquals("WPL_PalletID correct", "PLT_1", putawayLines1[0].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", false, putawayLines1[0].WPL_IsPuttingAway);
			AssertEquals("WPL_PalletID correct", "PLT_2", putawayLines1[1].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", false, putawayLines1[1].WPL_IsPuttingAway);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_1", true);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals(ErrorTypes.None, response1.Error);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ValidatePalletIDOnPutaway("PLT_2", true);
			AssertSuccessfulResponse(response2, webService);
			AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals(ErrorTypes.None, response2.Error);

			var otherFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJobAfter = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory2, data.Whs1, staff);
			AssertNotNull(putawayJobAfter);
			var putawayLinesAfter = putawayJobAfter.Lines;
			AssertEquals("PutawayJob Lines count correct.", 0, putawayLinesAfter.Count);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_PopulatesPalletInfo

		[TestDate(2023, 1, 1)]
		public void TestValidatePalletIDOnPutaway_PopulatesPalletInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			data.Org1.MiscServ.OM_IMUsePackingDate = true;
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1.PK, 1m, ZGuid.Empty, "PLT_1", new ZDate(2023, 1, 1), new ZDate(2022, 1, 1), "A1", "A2", "A3", "SN1", "");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response1 = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponse(response1, webService);
			AssertEquals("P1", response1.PalletInfo.ProductCode);
			AssertEquals("Attr1", response1.PalletInfo.PartAttrib1Name);
			AssertEquals("A1", response1.PalletInfo.PartAttrib1);
			AssertEquals("Attr2", response1.PalletInfo.PartAttrib2Name);
			AssertEquals("A2", response1.PalletInfo.PartAttrib2);
			AssertEquals("Attr3", response1.PalletInfo.PartAttrib3Name);
			AssertEquals("A3", response1.PalletInfo.PartAttrib3);
			AssertEquals("SN1", response1.PalletInfo.SerialNumber);
			AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), response1.PalletInfo.ExpiryDate);
			AssertEquals(new DateTime(2022, 1, 1).ToShortDateString(), response1.PalletInfo.PackingDate);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_CrossDock

		public void TestValidatePalletIDOnPutaway_CrossDock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response.Error);

			AssertEquals("Inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory.Location);
		}

		public void TestValidatePalletIDOnPutaway_CrossDock_WithAllocatedCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);

			inventory.WI_WL = crossDockLocation.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition: location should not be empty.", crossDockLocation, inventory.Location);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.None, response.Error);

			AssertEquals("Inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory.Location);

			var transfer = (WhsTransfer)inventory.AllPickLines.First().DocketLine.Docket;
			var transferLine = transfer.Lines[0];
			AssertEquals(crossDockLocation.PK, transferLine.WE_WL);
		}

		public void TestValidatePalletIDOnPutaway_CrossDock_InventoryReceivedOnDockdoorSameAsTheCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_WL_CrossDock = data.Whs1.DefaultInboundDockDoorLocation.PK;
			var reservedLine = order.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, inventory.WI_InventoryStatus);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PL1", false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			Assert("Invalid Cross Dock", response.ErrorMessage.Contains("Invalid cross dock putaway."));
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_CheckDGLimitCapacity

		public void TestValidatePalletIDOnPutaway_CheckDGLimitCapacity_DGExceedsWarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 15m);
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(data.Whs1);

			// Assert
			var response = webService.ValidatePalletIDOnPutaway("Pallet-1", false);
			AssertBusinessValidationError(webService, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", response);
		}

		public void TestValidatePalletIDOnPutaway_CheckDGLimitCapacity_DGUnderWarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 6m);
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(data.Whs1);

			// Assert
			var response = webService.ValidatePalletIDOnPutaway("Pallet-1", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestValidatePalletIDOnPutaway_CheckDGLimitCapacity_DGExceedsWarehouseLimit_MultipleSourceDocket()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive3, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(data.Whs1);

			// Assert
			var response = webService.ValidatePalletIDOnPutaway("Pallet-1", false);
			AssertBusinessValidationError(webService, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", response);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_TaskManagement

		public void TestValidatePalletIDOnPutaway_TaskManagement_CreateTask_NoPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transfer = ((WhsReceiveLine)inventory.InDocketLine).PutawayTransfer;
			var tasks = Helper.Factory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.PutawayJob));
			AssertNotNull(tasks);
			AssertEquals("Should be only 1 task created", 1, tasks.Length);
			AssertEquals("Create task parent correct", transfer.PK, tasks[0].P9_ParentID);
			AssertEquals("Response task PK correct", tasks[0].PK, response.TaskPK);

			var transferLine = transfer.Lines.Single();
			AssertEquals("TransferLine task correct", tasks[0].PK, transferLine.WE_P9_Task);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, tasks[0].P9_Status);
		}

		public void TestValidatePalletIDOnPutaway_TaskManagement_CreateTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var tasks = Helper.Factory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.PutawayJob));
			AssertNotNull(tasks);
			AssertEquals("Should be only 1 task created", 1, tasks.Length);
			AssertEquals("Create task parent correct", putawayTransfer.PK, tasks[0].P9_ParentID);
			AssertEquals("Response task PK correct", tasks[0].PK, response.TaskPK);
			AssertEquals("TransferLine task correct", tasks[0].PK, transferLine.WE_P9_Task);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, tasks[0].P9_Status);
		}

		public void TestValidatePalletIDOnPutaway_OpenTaskExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine.RunPreSaveValidation();
			var existingTask = Helper.CreateProcessTaskForTransfer(putawayTransfer);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Response task PK correct", existingTask.PK, response.TaskPK);
			AssertEquals("Task user correct", staff.GS_Code, existingTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, existingTask.P9_Status);
			AssertEquals("transferLine task correct", existingTask.PK, transferLine.WE_P9_Task);
		}

		public void TestValidatePalletIDOnPutaway_TaskExists_Suspended()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine.RunPreSaveValidation();
			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Response task PK correct", task.PK, response.TaskPK);
			AssertEquals("Task user correct", staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("transferLine task correct", task.PK, transferLine.WE_P9_Task);
		}

		public void TestValidatePalletIDOnPutaway_TaskExists_AssignedToAnotherUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine.RunPreSaveValidation();
			var taskForOtherUser = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", true);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("TaskForOtherUser is deleted", true, taskForOtherUser.IsDeleted);

			var staff1Tasks = Helper.Factory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff1.GS_Code));
			AssertNotNull(staff1Tasks);
			AssertEquals("Should be only 1 task created", 1, staff1Tasks.Length);
			AssertEquals("Create task parent correct", putawayTransfer.PK, staff1Tasks[0].P9_ParentID);
			AssertEquals("Response task PK correct", staff1Tasks[0].PK, response.TaskPK);
			AssertEquals("TransferLine task correct", staff1Tasks[0].PK, transferLine.WE_P9_Task);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, staff1Tasks[0].P9_Status);
		}

		public void TestValidatePalletIDOnPutaway_TaskExists_AnotherUserWorking()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine.RunPreSaveValidation();
			var taskForOtherUser = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff2);
			taskForOtherUser.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertBusinessValidationError(webService, "Scanned Pallet ID has a working task assigned to another user.", response);
			AssertEquals("Response task PK correct", Guid.Empty, response.TaskPK);

			AssertEquals("Task user correct", staff2.GS_Code, taskForOtherUser.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, taskForOtherUser.P9_Status);
			AssertEquals("transferLine task correct", taskForOtherUser.PK, transferLine.WE_P9_Task);
		}

		public void TestValidatePalletIDOnPutaway_TaskExists_UsesCurrentLineTaskOverSuspended()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT2", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT3", 10m);
			Helper.Factory.Save();

			var putawayTransfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT1", 10m);
			var task1 = Helper.CreateProcessTaskForTransfer(putawayTransfer1, staff);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			transferLine1.RunPreSaveValidation();

			var putawayTransfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT2", 10m);
			var task2 = Helper.CreateProcessTaskForTransfer(putawayTransfer2, staff);
			transferLine2.RunPreSaveValidation();

			var putawayTransfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2");
			putawayTransfer3.WD_IsPutawayTransfer = true;
			var transferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer3, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT3", 10m);
			transferLine3.RunPreSaveValidation();
			var task3 = Helper.CreateProcessTaskForTransfer(putawayTransfer3);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDOnPutaway("PLT2", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals("Response task PK correct", task2.PK, response.TaskPK);
			AssertEquals("Task user correct", staff.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			AssertEquals("TransferLine3 task correct", task2.PK, transferLine2.WE_P9_Task);

			AssertEquals("Task1 is not deleted", false, task1.IsDeleted);
			AssertEquals("TransferLine1 task correct", task1.PK, transferLine1.WE_P9_Task);
			AssertEquals("Task3 is not deleted", false, task3.IsDeleted);
			AssertEquals("TransferLine3 task correct", task3.PK, transferLine3.WE_P9_Task);
		}

		public void TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJob()
		{
			TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJobCore(assignedToAnotherUser: false);
		}

		public void TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJob_AssignedToAnotherUser()
		{
			TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJobCore(assignedToAnotherUser: true);
		}

		void TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJobCore(bool assignedToAnotherUser)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine1.RunPreSaveValidation();
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_2", 10m);
			transferLine2.RunPreSaveValidation();
			var existingTask = Helper.CreateProcessTaskForTransfer(putawayTransfer, assignedToAnotherUser ? staff2 : null);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var staffTasks = Helper.Factory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff1.GS_Code));
			AssertNotNull(staffTasks);
			AssertEquals("Should be only 1 task created", 1, staffTasks.Length);
			AssertEquals("Create task parent correct", putawayTransfer.PK, staffTasks[0].P9_ParentID);
			AssertEquals("Response task PK correct", staffTasks[0].PK, response.TaskPK);
			AssertEquals("TransferLine task correct", staffTasks[0].PK, transferLine1.WE_P9_Task);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, staffTasks[0].P9_Status);

			AssertEquals("TransferLine2 task status correct", assignedToAnotherUser ? staff2.GS_Code : string.Empty, existingTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("TransferLine2 task status correct", assignedToAnotherUser ? ProcessTaskStatusCodeList.Codes.Assigned : ProcessTaskStatusCodeList.Codes.Open, existingTask.P9_Status);
			AssertEquals("TransferLine2 task status correct", existingTask.PK, transferLine2.WE_P9_Task);
		}

		public void TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJob_AnotherUserWorking()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_1", 10m);
			transferLine1.RunPreSaveValidation();
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT_2", 10m);
			transferLine2.RunPreSaveValidation();
			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff2);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDOnPutaway("PLT_1", false);
			AssertBusinessValidationError(webService, "Scanned Pallet ID has a working task assigned to another user.", response);
			AssertEquals("Response task PK should be empty", Guid.Empty, response.TaskPK);

			AssertEquals("Original task user correct", staff2.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Original task status correct", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("Original transferLine1 task correct", task.PK, transferLine1.WE_P9_Task);
			AssertEquals("Original transferLine2 task correct", task.PK, transferLine2.WE_P9_Task);

			var query = new ZQuery(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.PutawayJob);
			query.AddToFilter(ProcessTasksSchema.PK, SQLComparisonOperator.NotEqual, task.PK);
			var createdTasks = Helper.Factory.Load<WhsTransferProcessTasks>(query);
			AssertNotNull(createdTasks);
			AssertEquals("No additional tasks should be created", 0, createdTasks.Length);
		}

		public void TestValidatePalletIDOnPutaway_TaskOnMultiPutawayJob_TakingPalletFromOtherwiseCompleteTask()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT2", 10m);
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT1", 10m);
			transferLine1.RunPreSaveValidation();
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT2", 10m);
			transferLine2.RunPreSaveValidation();

			var task1 = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff2);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Helper.Factory.Save();

			transferLine1.FinaliseDocketLine();
			Helper.Factory.Save();

			AssertEquals("Precondition: transferLine1 is finalised", true, transferLine1.IsFinalised);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDOnPutaway("PLT2", false);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Task1 is not deleted", false, task1.IsDeleted);
			AssertEquals("Task1 status correct", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals("TransferLine1 task correct", task1.PK, transferLine1.WE_P9_Task);

			var staff1Tasks = Helper.Factory.Load<WhsTransferProcessTasks>(new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, staff1.GS_Code));
			AssertNotNull(staff1Tasks);
			AssertEquals("Should be only 1 task created", 1, staff1Tasks.Length);
			AssertEquals("Create task parent correct", putawayTransfer.PK, staff1Tasks[0].P9_ParentID);
			AssertEquals("Response task PK correct", staff1Tasks[0].PK, response.TaskPK);
			AssertEquals("TransferLine task correct", staff1Tasks[0].PK, transferLine2.WE_P9_Task);
			AssertEquals("Task status correct", ProcessTaskStatusCodeList.Codes.Working, staff1Tasks[0].P9_Status);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetValidatePalletIDWebServiceResponse(WhsSecureService secureService, string palletID)
			=> secureService.ValidatePalletIDOnPutaway(palletID, false);

		protected override string UnfinalisedReceiptError() => "Pallet ID 12345 does not exist on an Un-Finalized Receipt.";

		protected override string BondedPalletError() => "Bonded Pallet should be unloaded on the desktop.";

		protected override void AssertPutawayTransferLine(WhsTransferLine line, OrgSupplierPart product, WhsLocation sourceLocation, WhsLocation destinationLocation, string palletID, decimal quantity, GlbStaff pickedBy, ZDateTimeOffset pickedTime)
		{
			CombineAssertions(() =>
			{
				AssertEquals(product.PK, line.WE_OP);
				AssertEquals(sourceLocation.PK, line.WE_WL_TransferFrom);
				AssertEquals(destinationLocation?.PK ?? ZGuid.Empty, line.WE_WL);
				AssertEquals(palletID, line.WE_TransferFromPalletId);
				AssertEquals(palletID, line.WE_PalletID);
				AssertEquals(InventoryStatus.Codes.PuttingAway, line.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.PuttingAway, line.WE_CurrentInventoryStatus);
				AssertEquals(quantity, line.QtyCommittedIncludingMatchingLines);
				AssertEquals(pickedBy, line.PickedBy);
				AssertEquals(pickedTime, line.PickedTime);
			});
		}

		#endregion
	}
}
