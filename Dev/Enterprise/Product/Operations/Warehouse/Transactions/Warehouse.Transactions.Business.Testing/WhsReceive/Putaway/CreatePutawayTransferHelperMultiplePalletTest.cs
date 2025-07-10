using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class CreatePutawayTransferHelperMultiplePalletTest : CreatePutawayTransferHelperSharedTest
	{
		#region TestValidatePalletIDForMultiplePutaway

		public void TestValidatePalletIDForMultiplePutaway()
		{
			var year = ZDateTime.Today.Year - 2;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			data.Org1.MiscServ.OM_IMUsePackingDate = true;
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "12345";
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 04, 02),
				new ZDate(year, 04, 01), "P1A1", "P1A2", "P1A3", "");
			inventory1.WI_PalletID = "PalletID1";
			inventory1.WI_WL = data.Whs1.WW_DefaultInboundDockDoor;
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 05, 02),
				new ZDate(year, 05, 01), "P1A1", "P1A2", "P1A3", "");
			inventory2.WI_PalletID = "PalletID2";
			inventory2.WI_WL = data.Whs1.WW_DefaultInboundDockDoor;
			Helper.Factory.Save();

			var message1 = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			var transfer = Factory.Load<WhsTransfer>(inventory1.PutawayTransferLine.WE_WD);
			CombineAssertions(() =>
			{
				AssertNotNull(transfer);
				AssertNull(message1);
				AssertEquals("Putaway Transfer has correct lines count", 1, transfer.Lines.Count);
				AssertEquals("Putaway TransferLine WE_GS_PutawayBy correct.", staff.GS_Code, transfer.Lines[0].WE_GS_NKPutawayBy);
			});

			var message2 = CreateTransfer(data.Whs1, ["PalletID2"], staff.GS_Code);
			CombineAssertions(() =>
			{
				AssertNull(message2);
				AssertEquals("Putaway Transfer has correct lines count", 2, transfer.Lines.Count);
				AssertEquals("Putaway TransferLine PalletID correct.", "PalletID1", transfer.Lines[0].WE_PalletID);
				AssertEquals("Putaway TransferLine PalletID correct.", "PalletID2", transfer.Lines[1].WE_PalletID);
			});
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletIDsAndDestinationLocations()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_3");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_4");
			Helper.Factory.Save();

			var saveCount = 0;
			Factory.Saving += _ => saveCount++;

			var message = CreateTransfer(data.Whs1, new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, staff.GS_Code);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertNull(message);
				AssertEquals("Should be 1 Save", 1, saveCount);
			});
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_WithCancelledPalletIDs

		public void TestValidatePalletIDForMultiplePutaway_WithCancelledPalletIDs()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_2");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_3");
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
			AssertNotNull(inventory.PutawayTransferLine);
			AssertNull(message);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_OnePutawayTransfer_DBHits

		public void TestValidatePalletIDForMultiplePutaway_OnePutawayTransfer_DBHits()
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
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
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
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
				};
			});
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_CrossDock

		public void TestValidatePalletIDForMultiplePutaway_CrossDock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine1.ReservedQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 15m);
			order2.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Stock is reserved.", 15m, reservedLine2.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);

			var message = CreateTransfer(data.Whs1, new[] { "PL1", "PL2" }, staff.GS_Code);
			AssertNull(message);

			AssertEquals("Inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory1.Location);
			AssertEquals("Inventory has putaway transfer.", true, inventory2.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory2.Location);
		}

		public void TestValidatePalletIDOnPutaway_CrossDock_WithAllocatedCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine1.ReservedQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 15m);
			order2.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Stock is reserved.", 15m, reservedLine2.ReservedQuantity);

			inventory1.WI_WL = crossDockLocation.PK;
			inventory2.WI_WL = crossDockLocation.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition: location should not be empty.", crossDockLocation, inventory1.Location);
			AssertEquals("Precondition: location should not be empty.", crossDockLocation, inventory2.Location);

			var message = CreateTransfer(data.Whs1, new[] { "PL1", "PL2" }, staff.GS_Code);
			AssertNull(message);

			AssertEquals("Inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory1.Location);
			AssertEquals("Inventory has putaway transfer.", true, inventory2.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory2.Location);

			var transfer2 = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			AssertEquals(crossDockLocation.PK, transfer2.Lines[0].WE_WL);
			AssertEquals(crossDockLocation.PK, transfer2.Lines[1].WE_WL);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity

		public void TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity_DGExceedsWarehouseLimit()
		{
			// Arrange
			var staff = Helper.CreateGlbStaff("S1", "S1");
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
			receive1.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2", 6m);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, new[] { "Pallet-1", "Pallet-2" }, staff.GS_Code);
			AssertEquals(@"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", message);
		}

		public void TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity_DGUnderWarehouseLimit()
		{
			// Arrange
			var staff = Helper.CreateGlbStaff("S1", "S1");
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
			receive1.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2", 5m);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, new[] { "Pallet-1", "Pallet-2" }, staff.GS_Code);
			AssertNull(message);
		}

		public void TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity_DGExceedsWarehouseLimit_MultipleSourceDocket()
		{
			// Arrange
			var staff = Helper.CreateGlbStaff("S1", "S1");
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
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 3m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2", 4m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive3, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-3", 5m);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, new[] { "Pallet-1", "Pallet-2", "Pallet-3" }, staff.GS_Code);
			AssertEquals(@"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", message);
		}

		#endregion

		#region Implementation

		protected override bool IsMultiPalletPutaway => true;

		#endregion
	}
}
