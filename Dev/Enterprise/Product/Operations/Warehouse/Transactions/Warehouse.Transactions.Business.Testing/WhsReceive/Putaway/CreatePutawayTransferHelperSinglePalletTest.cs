using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class CreatePutawayTransferHelperSinglePalletTest : CreatePutawayTransferHelperSharedTest
	{
		#region TestCreateTransfer

		public void TestPutawayAndUnloadAtSameTime()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "12345", 10m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);

			CreateTransfer(data.Whs1, ["12345"], staff.GS_Code);
			AssertEquals("Precondition", true, inventory1.HasPutawayTransfer);

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW2", ZDateTimeOffset.Empty);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation, "12345", 20m);
			Helper.Factory.Save();

			AssertEquals("Inventory with the same pallet id on the same location can be created.", false, inventory2.HasErrors);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);

			var message = CreateTransfer(data.Whs1, ["12345"], staff.GS_Code);
			AssertEquals("Putaway Failed: Inventories with Pallet ID 12345 are currently being putaway and unloaded at the same time.", message);
		}

		public void TestInvalidInventoryStatus()
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

			var message = CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			AssertEquals("Putaway transfer creation failed because Pallet ID 12345 must be entirely either Arrived, Received to Dock Door or Putaway.", message);
		}

		public void TestPalletBelongToMultipleClients()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var org2 = Helper.CreateClient("222", "222");

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "12345", 10m);

			var receive2 = Helper.CreateWhsReceive(org2.PK, data.Whs1.PK, "INW2", ZDateTimeOffset.Empty);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation, "12345", 20m);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			AssertEquals("Putaway transfer creation failed because this Pallet ID 12345 has products belong to multiple clients.", message);
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
			Factory.Save();
			data.Whs1.WW_DefaultInboundDockDoor = defaultInboundDockDoor.PK;

			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location, "PLT1");
			inventory2.WI_ArrivalDate = DateTime.Now;
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, inventory2.WI_InventoryStatus);
			AssertNotEquals("Precondition: Inventory3 location is not default inbound DockDoor.", data.Whs1.DefaultInboundDockDoorLocation.ToLocationString(), location.ToLocationString());

			var message = CreateTransfer(data.Whs1, ["PLT1"], staff1.GS_Code);
			AssertEquals("Putaway transfer creation failed because Pallet ID PLT1 must be entirely either Arrived, Received to Dock Door or Putaway.", message);
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

			var message = CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			AssertEquals("Putaway transfer creation failed because Pallet ID 12345 must be entirely either Arrived, Received to Dock Door or Putaway.", message);
		}

		#endregion

		#region TestWithCancelledPalletIDs

		public void TestWithCancelledPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

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

			var message = CreateTransfer(data.Whs1, ["PLT_3"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);
		}

		#endregion

		#region TestCrossDock

		public void TestCrossDock()
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

			CreateTransfer(data.Whs1, ["PL1"], staff.GS_Code);
			AssertEquals("Inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory.Location);
		}

		public void TestCrossDock_WithAllocatedCrossDockLocation()
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

			CreateTransfer(data.Whs1, ["PL1"], staff.GS_Code);
			AssertEquals("Inventory has putaway transfer.", true, inventory.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory.Location);

			var transfer = (WhsTransfer)inventory.AllPickLines.First().DocketLine.Docket;
			var transferLine = transfer.Lines[0];
			AssertEquals(crossDockLocation.PK, transferLine.WE_WL);
		}

		public void TestCrossDock_InventoryReceivedOnDockdoorSameAsTheCrossDockLocation()
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

			var message = CreateTransfer(data.Whs1, ["PL1"], staff.GS_Code);
			Assert("Invalid Cross Dock", message.Contains("Invalid cross dock putaway."));
		}

		#endregion

		#region TestUNDG

		public void TestCheckDGLimitCapacity_DGExceedsWarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var staff = Helper.CreateGlbStaff("S1", "S1");
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
			var message = CreateTransfer(warehouse, ["Pallet-1"], staff.GS_Code);

			// Assert
			AssertEquals(@"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", message);
		}

		public void TestCheckDGLimitCapacity_DGUnderWarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var staff = Helper.CreateGlbStaff("S1", "S1");
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
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 6m);
			Helper.Factory.Save();

			// Act
			var message = CreateTransfer(warehouse, ["Pallet-1"], staff.GS_Code);

			// Assert
			AssertNotNull(inventory.PutawayTransferLine);
			AssertNull(message);
		}

		public void TestCheckDGLimitCapacity_DGExceedsWarehouseLimit_MultipleSourceDocket()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var staff = Helper.CreateGlbStaff("S1", "S1");
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
			var message = CreateTransfer(warehouse, ["Pallet-1"], staff.GS_Code);

			// Assert
			AssertEquals(@"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", message);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHits

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
					{ OrgHeaderSchema.Constants.TableName, 2 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },
					{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 2 },
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 3 },
					{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
				};
			});
		}

		#endregion

		#region Implementation

		protected override bool IsMultiPalletPutaway => false;

		#endregion
	}
}
