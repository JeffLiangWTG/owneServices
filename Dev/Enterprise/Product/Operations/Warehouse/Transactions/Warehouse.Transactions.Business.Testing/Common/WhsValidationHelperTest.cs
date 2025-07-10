using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLineDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsValidationHelperTest : WhsTestCaseWithFactory
	{
		#region PerformanceTest

		public void TestDBHits_FinalizeReceiveLinesWithPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "P0001");
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsPickFaceSchema.Constants.TableName, 1 }, // Due to checking is location is PickFace
				{ WhsPutawayJobSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				var receiveInOtherFactory = newfactory.Load<WhsReceive>(receive.PK);
				receiveInOtherFactory.FinaliseDocketWithoutUserConfirmation();
				Assert("Receive should be finalized.", receiveInOtherFactory.IsFinalised);
			}
		}

		public void TestDBHits_RunPreSaveValidationForReceivesLineWithPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "P0001");
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveInOtherFactory = newfactory.Load<WhsReceive>(receive.PK);
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.RunPreSaveValidation();
			}

			CombineAssertions(() =>
			{
				AssertDbHits(expectedDbHits, newfactory);
				Assert("Receive should have no error.", !receiveInOtherFactory.HasErrors());
			});
		}

		public void TestDBHits_ValidateReceiveLineWithPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "P0001");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "P0001");

			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveInOtherFactory = newfactory.Load<WhsReceive>(receive.PK);
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
			};

			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.Lines[0].Validation.ValidateWE_PalletID();
				receiveInOtherFactory.Lines[1].Validation.ValidateWE_PalletID();
			}

			AssertDbHits(expectedDbHits, newfactory);
		}

		public void TestDBHits_RunPreSaveValidationWithFetchHints_SOHLocationWarningEnabled()
		{
			const int NumberOfUniqueRecords = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			for (int i = 0; i < NumberOfUniqueRecords; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, new ZDecimal(i + 1), data.Whs1.DefaultLocation);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveInOtherFactory = newFactory.Load<WhsReceive>(receive.PK);
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			// enable SOH location warning to check location stock on hand during location validation
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.RunPreSaveValidationWithFetchHints();
			}
		}

		#endregion

		#region TestCheckIfTrimIsNeeded

		public void TestCheckIfTrimIsNeeded()
		{
			var myObject = new BusinessObjectForTest();
			myObject.PalletId = "name does not need to be trimmed";
			AssertNoError("Does not need to be trimmed", myObject.PalletIdInfo, WhsValidationHelper.ValueHasToBeTrimmed);

			myObject.PalletId = "name needs to be trimmed ";
			AssertHasError("Need to be trimmed at the end", myObject.PalletIdInfo, WhsValidationHelper.ValueHasToBeTrimmed);

			myObject.PalletId = " name needs to be trimmed";
			AssertHasError("Need to be trimmed at the beginning", myObject.PalletIdInfo, WhsValidationHelper.ValueHasToBeTrimmed);
		}

		#endregion

		#region TestGetNotEnoughStockMessage

		public void TestGetNotEnoughStockMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1-1", "A-1-2");

			var expectedErrorMessage1 =
@"Attempted to transfer 10 Units, but only 10 Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";
			AssertEquals(expectedErrorMessage1, WhsValidationHelper.GetNotEnoughStockMessage(transferLine, 10m));

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var expectedErrorMessage2 =
@"Attempted to transfer 10 Units, but no Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the attributes exactly match the attributes on the inventory you are trying to transfer.
If you are trying to transfer stock with attributes, you must enter the attribute exactly. Blank non mandatory attributes will only match to inventory with blank attributes.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.";
			AssertEquals(expectedErrorMessage2, WhsValidationHelper.GetNotEnoughStockMessage(transferLine, 0m));

			// US Bonded Message
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var expectedErrorMessage3 =
@"Attempted to transfer 10 Units, but no Units are available for transfer out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the transfer line.
Check that all the attributes exactly match the attributes on the inventory you are trying to transfer.
If you are trying to transfer stock with attributes, you must enter the attribute exactly. Blank non mandatory attributes will only match to inventory with blank attributes.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to transfer.
If you are trying to transfer stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to transfer stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.
If you are trying to transfer stock with a Package Group ID you must enter it exactly. Blank Package Group IDs will only match to inventory with blank Package Group IDs.";
			AssertEquals(expectedErrorMessage3, WhsValidationHelper.GetNotEnoughStockMessage(transferLine, 0m));
		}

		#endregion

		#region TestCheckPalletID

		public void TestCheckPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, true, false);
			var receiveLine = receive.Lines[0];

			using (receiveLine.GetValidationSuspender())
			{
				receiveLine.WE_PalletID = "  xxx";
			}
			using (receiveLine.SuspendValidationTesting())
			{
				WhsValidationHelper.CheckPalletID(receiveLine.WE_PalletIDInfo, receiveLine.WE_WL, receive, receiveLine, receiveLine.Inventory[0].ValidationPalletIDWarningMessage);
			}
			AssertHasErrors(WhsValidationHelper.ValueHasToBeTrimmed, receiveLine.WE_PalletIDInfo);

			using (receiveLine.GetValidationSuspender())
			{
				receiveLine.WE_PalletID = "";
			}
			using (receiveLine.SuspendValidationTesting())
			{
				receiveLine.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(receiveLine.WE_PalletIDInfo, receiveLine.WE_WL, receive, receiveLine, receiveLine.Inventory[0].ValidationPalletIDWarningMessage);
			}
			AssertNoErrors("Shouldn't validate if empty Pallet IDs", receiveLine.WE_PalletIDInfo);

			using (receiveLine.GetValidationSuspender())
			{
				receiveLine.WE_WL = ZGuid.Empty;
				receiveLine.WE_PalletID = "XXX123";
			}
			using (receiveLine.SuspendValidationTesting())
			{
				receiveLine.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(receiveLine.WE_PalletIDInfo, receiveLine.WE_WL, receive, receiveLine, receiveLine.Inventory[0].ValidationPalletIDWarningMessage);
			}
			AssertNoErrors("Shouldn't validate if empty location", receiveLine.WE_PalletIDInfo);

			AssertNoWarnings(receiveLine.WE_PalletIDInfo);
			receiveLine.Inventory[0].ValidationPalletIDWarningMessage = "Some Message";
			using (receiveLine.SuspendValidationTesting())
			{
				receiveLine.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(receiveLine.WE_PalletIDInfo, receiveLine.WE_WL, receive, receiveLine, receiveLine.Inventory[0].ValidationPalletIDWarningMessage);
			}
			AssertHasWarning("Should have warning we provided.", receiveLine.WE_PalletIDInfo, "Some Message");
		}

		#endregion

		#region TestCheckPalletID_InTransitInventory

		#region TestCheckPalletID_InTransit_Transfer

		public void TestCheckPalletID_InTransit_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 100m, locationA, "PLT456");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
			var transferLine2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 50m, locationA, "PLT456", locationB, "PLT456", picker);
			Factory.Save();

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive3Line = Helper.CreateWhsReceiveLine(receive3, data.Part1, 45m, locationB, "PLT111");
			AssertNoErrors("Receive should not have any errors.", receive3Line.WE_PalletIDInfo);

			receive3Line.WE_PalletID = "PLT123";
			using (receive3Line.SuspendValidationTesting())
			{
				receive3Line.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(receive3Line.WE_PalletIDInfo, receive3Line.WE_WL, receive3, receive3Line, "");
				AssertNoErrors("The receipt should NOT have any errors since pallet 123 is fully picked and in transit to locationB.", receive3Line.WE_PalletIDInfo);
			}

			// Should show an error if trying to receive into another location (e.g. the original source location)
			using (receive3Line.SuspendValidationTesting())
			{
				receive3Line.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(receive3Line.WE_PalletIDInfo, locationA.PK, receive3, receive3Line, "");
				AssertHasError("The receipt should have an error since pallet 123 is in transit to location 2.", receive3Line.WE_PalletIDInfo,
					"Another location (A-2) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			}

			receive3Line.WE_PalletID = "PLT456";
			using (receive3Line.SuspendValidationTesting())
			{
				receive3Line.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(receive3Line.WE_PalletIDInfo, receive3Line.WE_WL, receive3, receive3Line, "");
				AssertHasError("The receipt should have an error since pallet 456 still has stock on hand.", receive3Line.WE_PalletIDInfo,
					"Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.");
			}
		}

		#endregion

		#region TestCheckPalletID_InTransit_Pick

		public void TestCheckPalletID_InTransit_Pick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			inventory1.WI_PalletID = "PLT1";
			inventory1.WI_WL = locations[0].PK;
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Stock On Hand.", 10m, inventory1.WI_TotalUnits);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition: No Stock On Hand.", 0m, inventory1.WI_TotalUnits);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			inventory2.WI_PalletID = "PLT1";
			inventory2.WI_WL = locations[1].PK;

			var inventory2Line = inventory2.InDocketLine;
			using (inventory2Line.SuspendValidationTesting())
			{
				inventory2Line.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(inventory2Line.WE_PalletIDInfo, inventory2Line.WE_WL, receive2, inventory2Line, "");
			}

			AssertHasError("Should have error for In-Transit stock with the same pallet ID in DockDoor location.", inventory2Line.WE_PalletIDInfo, "Another location (DOCKDOOR) was already used for the same Pallet ID. Please select another location or Pallet ID.");

			inventory2.WI_PalletID = "PLT2";
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			using (inventory2Line.SuspendValidationTesting())
			{
				inventory2Line.WE_PalletIDInfo.ClearAllNotifications();
				WhsValidationHelper.CheckPalletID(inventory2Line.WE_PalletIDInfo, inventory2Line.WE_WL, receive2, inventory2Line, "");
			}
			AssertNoErrors("Should *not* have error when Pallet is fully picked.", inventory2Line.WE_PalletIDInfo);
		}

		#endregion

		public void TestCheckPalletID_InTransitInventory_PartialPickIsNotAllowed()
		{
			// Verify that finalize is prevented when trying to transfer partial pallet inventory in transit.
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedErrorMsg = "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
			var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 20m, locationA, "PLT123", locationB, "PLT123", picker);
			AssertNoErrors("Precondition: Inventory must be set In Transit.", transferLine1.WE_PalletIDInfo);
			transfer.FinaliseDocket();

			AssertEquals("Inventory must be In Transit", InventoryStatus.Codes.InTransit, transferLine1.WE_OriginalInventoryStatus);
			AssertEquals("TransferLine must NOT be finalised when pallet was partially picked.", false, transferLine1.IsFinalised);
			AssertHasError("TransferLine must have validation errors when pallet was partially picked.", transferLine1.WE_PalletIDInfo, expectedErrorMsg);

			var transferLine2 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer, data.Part1, 80m, locationA, "PLT123", locationB, "PLT123", picker);
			transfer.FinaliseDocket();

			AssertEquals("Inventory must be Available for Line 1", InventoryStatus.Codes.Available, transferLine1.WE_OriginalInventoryStatus);
			AssertEquals("Inventory must be Available for Line 2", InventoryStatus.Codes.Available, transferLine2.WE_OriginalInventoryStatus);
			AssertEquals("Transfer must be finalised when full pallet was picked.", true, transfer.IsFinalised);
			AssertNoErrors("Transfer must NOT have validation errors when full pallet was picked.", transferLine1.WE_PalletIDInfo);
		}

		public void TestCheckPalletID_InTransitInventory_StockOnHandWarning()
		{
			// Verify warning with indication that we have stock on hand shows when moving inventory to a location with an expected "In-Transit" putaway from different pallet.
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var locationA = data.Whs1.FindLocation("A-1");
			var locationB = data.Whs1.FindLocation("A-2");
			var picker = Helper.CreateGlbStaff("RSL", "Russell");
			var expectedWarningMsg = "Stock On Hand exists.\r\nClient: 111, Product: P1";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 100m, locationA, "PLT456");
			Factory.Save();

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
				var transferLine1 = Helper.CreateWhsTransferLineWithInTransitInventory(transfer1, data.Part1, 100m, locationA, "PLT123", locationB, "PLT123", picker);
				Factory.Save();

				AssertNoWarnings("Precondition: Transfer should show no warnings.", transferLine1.LocationStringInfo);

				var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Transfer1");
				var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 100m, locationA.WLV_LocationString, "PLT456", locationB.WLV_LocationString, "PLT456");
				transferLine2.RunPreSaveValidation();
				AssertHasWarning("Transfer should show warning since PLT123 is already in transit to that location.", transferLine2.LocationStringInfo, expectedWarningMsg);

				transferLine2.WE_PalletID = "PLT123";
				transferLine2.RunPreSaveValidation();
				AssertNoWarnings("Transfer should NOT show warnings since we're adding inventory to PLT123", transferLine2.LocationStringInfo);
			}
		}

		#endregion

		#region TestGetDuplicatePalletIdMessage

		public void TestGetDuplicatePalletIdMessage_Location()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var message = WhsValidationHelper.GetDuplicatePalletIdMessage(data.Whs1.FindLocation("A-1"));
			AssertEquals("Message returned is correct", "Another location (A-1) was already used for the same Pallet ID. Please select another location or Pallet ID.", message);
		}

		public void TestGetDuplicatePalletIdMessage_LocationString()
		{
			var message = WhsValidationHelper.GetDuplicatePalletIdMessage("J-77");
			AssertEquals("Message returned is correct", "Another location (J-77) was already used for the same Pallet ID. Please select another location or Pallet ID.", message);
		}

		#endregion

		#region BusinessObjectForTest

		public class BusinessObjectForTest : NonPersistentBusinessObject
		{
			#region PalletId

			public ZString PalletId
			{
				get { return palletId; }
				set
				{
					palletId = value;
					Validation.ValidatePalletId();
					PalletIdInfo.RefreshBinding();
				}
			}
			ZString palletId;

			public ZPropertyInfo PalletIdInfo
			{
				get
				{
					return this.GetZPropertyInfo(nameof(PalletId));
				}
			}

			#endregion

			public BusinessObjectForTestValidation Validation
			{
				get { return new BusinessObjectForTestValidation(this); }
			}
		}

		#region BusinessObjectForTestValidation

		public class BusinessObjectForTestValidation : ZValidation
		{
			public BusinessObjectForTestValidation(BusinessObjectForTest parent)
				: base(parent)
			{
				Parent = parent;
			}

			readonly BusinessObjectForTest Parent;

			#region AutoValidationType

			public override Type AutoValidationType
			{
				get { return typeof(BusinessObjectForTestValidation); }
			}

			#endregion

			#region ValidatePalletId

			public override void ValidateAll()
			{
				ValidatePalletId();
			}

			public void ValidatePalletId()
			{
				ValidateCalculatedProperty(Parent.PalletIdInfo);
			}

			protected void CheckPalletId()
			{
				WhsValidationHelper.CheckIfTrimIsNeeded(Parent.PalletIdInfo);
			}

			#endregion
		}

		#endregion

		#endregion

		#region Location

		#region TestCheckWI_WL

		public void TestCheckWI_WL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				locationA2.WLV_LocationStatus = LocationStatus.Codes.Void;

				var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1).InDocketLine;
				inventoryLine.WE_WL = ZGuid.Empty;
				AssertHasError(inventoryLine.WE_WLInfo, "Please enter a valid location.");

				inventoryLine.WE_WL = ZGuid.Invalid;
				AssertHasError(inventoryLine.WE_WLInfo, "Enter a valid Location.");
				AssertNoError(inventoryLine.WE_WLInfo, "Please enter a valid location.");

				inventoryLine.WE_WL = locationA1.PK;
				AssertNoErrors(inventoryLine.WE_WLInfo);

				inventoryLine.WE_WL = locationA2.PK;
				AssertHasError(inventoryLine.WE_WLInfo, "Please enter a valid location, the Location you entered is Void.");
			}
		}

		public void TestCheckWI_WL_Error_ValidLocationZeroUnit_Finalising()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1).InDocketLine;

				inventoryLine.WE_TransactionQuantity = 0m;
				inventoryLine.WE_WL = locationA1.PK;
				AssertNoErrors(inventoryLine.WE_WLInfo);
				inventoryLine.WE_WL = ZGuid.Empty;
				AssertNoErrors(inventoryLine.WE_WLInfo);
			}
		}

		#endregion

		#region TestCheckWI_WL_Finalised

		public void TestCheckWI_WL_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA1 = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1).InDocketLine;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertIsFinalisedPrecondition(order);
			AssertNoErrors("Precondition", inventoryLine.WE_WLInfo);

			locationA1.WLV_LocationStatus = LocationStatus.Codes.Void;
			inventoryLine.Validation.ValidateWE_WL();
			AssertNoErrors("Finalised Receive Should Not Validation Locations.", inventoryLine.WE_WLInfo);
		}

		#endregion

		#region TestCheckWI_WL_LocationMaxWeightVolumeQuantity

		#region TestCheckWI_WL_LocationMaxWeight

		public void TestCheckWI_WL_LocationMaxWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[0], 0m, "", 0m, "");
			Helper.SetLocationMaxWeightAndVolume(locations[1], 100m, "KG", 0m, "");
			Helper.SetLocationMaxWeightAndVolume(locations[2], 0.1m, "T", 0m, "");

			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 0m, "");
			Helper.SetProductWeightAndVolume(data.Part2, 1000m, "G", 0m, "");

			AssertCheckWI_WL_LocationMaxWeightVolume(data, locations, new[] { "Total required Weight (", ") exceeds the maximum available Weight (", ") for this location." });
		}

		public void TestCheckWI_WL_LocationMaxWeightWithNoProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations[0];
			Helper.SetLocationMaxWeightAndVolume(location, 100m, "KG", 0m, "");
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 0m, "");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, ZGuid.Empty, 75m);
			AssertNoExceptionThrown("Null reference exception should not be thrown.", () => inventory.WI_WL = location.PK);
			AssertEquals("Location should be set correctly even without a product.", "A", inventory.LocationString);
		}

		#endregion

		#region TestCheckWI_WL_LocationMaxVolume

		public void TestCheckWI_WL_LocationMaxVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.SetLocationMaxWeightAndVolume(locations[0], 0m, "", 0m, "");
			Helper.SetLocationMaxWeightAndVolume(locations[1], 0m, "", 2m, "M3");
			Helper.SetLocationMaxWeightAndVolume(locations[2], 0m, "", 2000m, "D3");

			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0.02m, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 0m, "", 20m, "D3");

			AssertCheckWI_WL_LocationMaxWeightVolume(data, locations, new[] { "Total required Volume (", ") exceeds the maximum available Volume (", ") for this location." });
		}

		#endregion

		#region AssertCheckWI_WL_LocationMaxWeightVolume

		void AssertCheckWI_WL_LocationMaxWeightVolume(TestDataSimpleEnvironment data, WhsLocationCollection locations, string[] expectedPartsOfWarningMessage)
		{
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 75m).InDocketLine; // To overfloow location Max Weight you need to put > 100 units into it.
			var inventoryLine1_2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 50m).InDocketLine;
			var inventoryLine1_3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 75m).InDocketLine;

			inventoryLine1_1.WE_WL = locations[0].PK;
			AssertEquals(false, HasMessageThatContainParts(inventoryLine1_1.WE_WLInfo, expectedPartsOfWarningMessage));

			// locations[1] made up of 50 received + 25 transfered
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TR1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, "A-1", locations[1].ToLocationString());
			transfer.RunPreSaveValidation(); // to commit units.
			transfer.Factory.Save(); // don't finalise

			inventoryLine1_2.WE_WL = locations[1].PK;
			AssertEquals(false, HasMessageThatContainParts(inventoryLine1_2.WE_WLInfo, expectedPartsOfWarningMessage));

			inventoryLine1_3.WE_WL = locations[2].PK;
			AssertEquals(false, HasMessageThatContainParts(inventoryLine1_3.WE_WLInfo, expectedPartsOfWarningMessage));

			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventoryLine2_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 200m).InDocketLine;
			var inventoryLine2_2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 75m).InDocketLine;
			var inventoryLine2_3_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m).InDocketLine;
			var inventoryLine2_3_2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20m).InDocketLine;
			Factory.Save();

			inventoryLine2_1.WE_WL = locations[0].PK; // shouldn't generate warning since location has no max weight defination.
			AssertEquals(false, HasMessageThatContainParts(inventoryLine2_1.WE_WLInfo, expectedPartsOfWarningMessage));

			inventoryLine2_2.WE_WL = locations[1].PK; // overfloow by weight
			AssertEquals(true, HasMessageThatContainParts(inventoryLine2_2.WE_WLInfo, expectedPartsOfWarningMessage));

			inventoryLine2_3_1.WE_WL = locations[2].PK; // 95 out of 100 units allocated into this location no overfloow expected
			AssertEquals(false, HasMessageThatContainParts(inventoryLine2_3_1.WE_WLInfo, expectedPartsOfWarningMessage));

			inventoryLine2_3_2.WE_WL = locations[2].PK; // 95+20 out of 100 units allocated into this location overfloow expected.
			AssertEquals(false, HasMessageThatContainParts(inventoryLine2_3_1.WE_WLInfo, expectedPartsOfWarningMessage)); // old lines shouldn't get warning yet
			AssertEquals(true, HasMessageThatContainParts(inventoryLine2_3_2.WE_WLInfo, expectedPartsOfWarningMessage)); // only new lines get warning
		}

		bool HasMessageThatContainParts(ZPropertyInfo info, string[] expectedPartsOfTheWarningMessage, bool isErrorExpected = false)
		{
			var messages = isErrorExpected ? info.GetErrors() : info.GetWarnings();
			foreach (var notification in messages)
			{
				if (notification.Message.Contains(expectedPartsOfTheWarningMessage[0]) &&
					notification.Message.Contains(expectedPartsOfTheWarningMessage[1]) &&
					notification.Message.Contains(expectedPartsOfTheWarningMessage[2]))
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region TestCheckWI_WL_LocationMaxQuantity

		public void TestCheckWI_WL_LocationMaxQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_MaxQuantity = 0m;
			locations[1].WLV_MaxQuantity = 20m;
			locations[2].WLV_MaxQuantity = 20m;
			locations[3].WLV_MaxQuantity = 20m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]).InDocketLine;
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[1]).InDocketLine;
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[2]).InDocketLine;
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[3]).InDocketLine;

			AssertNoErrors(line1.WE_WLInfo);
			AssertNoErrors(line2.WE_WLInfo);
			AssertNoErrors(line3.WE_WLInfo);
			AssertNoErrors(line4.WE_WLInfo);

			Factory.Save();

			// transfer some stock to location 3
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "TR1", data.Part1, 5m, locations[4], "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, locations[4], locations[3]);
			transfer.RunPreSaveValidation(); // to commit units.
			transfer.Factory.Save(); // don't finalise

			// location 0 - 2
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 11m, locations[0]).InDocketLine; // = 21 (no max)
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[1]).InDocketLine; // = 20 (max 20)
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 11m, locations[2]).InDocketLine; // = 21 (max 20)
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 6m, locations[3]).InDocketLine;  // = 21 (including transfer)(max 20)
			receive2.RunPreSaveValidation();

			AssertNoErrors(receiveLine1.WE_WLInfo);
			AssertNoErrors(receiveLine2.WE_WLInfo);
			AssertHasError(receiveLine3.WE_WLInfo, "Total required Quantity (11) exceeds the maximum available Quantity (10.000) for this location.");
			AssertHasError(receiveLine4.WE_WLInfo, "Total required Quantity (6) exceeds the maximum available Quantity (5.000) for this location.");
		}

		#endregion

		#endregion

		#region TestCheckWI_WL_LocationSOH

		public void TestCheckWI_WL_LocationSOH()
		{
			// disable SOH location warning
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var recevie1Line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1).InDocketLine;
			var recevie1Line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, locationA1).InDocketLine;
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receive2Line1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m).InDocketLine;
			receive2Line1.WE_WL = locationA1.PK;
			AssertNoWarnings("Location should have no warnings.", receive2Line1.WE_WLInfo);

			// enable SOH location warning
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			receive2Line1.WE_WL = locationA2.PK;
			AssertEquals("Location A2 has no stock, there should be no warnings.", 0, receive2Line1.WE_WLInfo.GetWarnings().Count());

			receive2Line1.WE_WL = locationA1.PK;
			var warning = receive2Line1.WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}
Client: {0}, Product: {2}", data.Org1.OH_Code, recevie1Line1.ProductCode, recevie1Line2.ProductCode), warning);

			receive2Line1.WE_WL = locationA2.PK;
			AssertEquals("Location A2 has no stock, there should be no warnings.", 0, receive2Line1.WE_WLInfo.GetWarnings().Count());

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_P2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-3");
			transferLine_P2.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			receive2Line1.WE_WL = locationA3.PK;
			var warningAfterTranfer = receive2Line1.WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}", transfer.Client.OH_Code, transferLine_P2.ProductCode), warningAfterTranfer); // ignore self .. ie P1
		}

		public void TestCheckWI_WL_LocationSOH_InventoriesNotYetSavedInDB()
		{
			// enable SOH location warning
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var recevie1Line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1).InDocketLine;
			var recevie1Line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, locationA1).InDocketLine;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receive2Line1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m).InDocketLine;
			receive2Line1.WE_WL = locationA1.PK;
			var warning = receive2Line1.WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}
Client: {0}, Product: {2}", data.Org1.OH_Code, recevie1Line1.ProductCode, recevie1Line2.ProductCode), warning);

			receive2Line1.WE_WL = locationA2.PK;
			AssertEquals("Location A2 has no stock, there should be no warnings.", 0, receive2Line1.WE_WLInfo.GetWarnings().Count());

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine_P2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1", "A-3");

			receive2Line1.WE_WL = locationA3.PK;
			var warningAfterTransfer = receive2Line1.WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}", transfer.Client.OH_Code, transferLine_P2.ProductCode), warningAfterTransfer); // ignore self .. ie P1
		}

		public void TestCheckWI_WL_LocationSOH_SomeInventoriesWithUpdatesInMemory()
		{
			// enable SOH location warning
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var product3 = Helper.CreateProduct(data.Org1, "P3");
			var product4 = Helper.CreateProduct(data.Org1, "P4");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive1Line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1).InDocketLine;
			var receive1Line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 20m, locationA1).InDocketLine;
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receive2Line1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 30m).InDocketLine;
			receive2Line1.WE_WL = locationA1.PK;
			Factory.Save();

			receive2Line1.WE_OP = product3.PK; // update receive2Line1's product to include prouct 3 in the SOH warning

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			var receive3Line1 = Helper.CreateWhsReceiveInventoryLine(receive3, product4, 40m).InDocketLine;
			receive3Line1.WE_WL = locationA1.PK;
			var warning = receive3Line1.WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}
Client: {0}, Product: {2}
Client: {0}, Product: {3}", data.Org1.OH_Code, receive1Line1.ProductCode, receive1Line2.ProductCode, product3.OP_PartNum), warning);
		}

		public void TestCheckWI_WL_LocationSOH_SomeInventoriesWithUpdatesInMemory_DifferentFactory()
		{
			// enable SOH location warning
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var product3 = Helper.CreateProduct(data.Org1, "P3");
			var product4 = Helper.CreateProduct(data.Org1, "P4");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive1Line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, locationA1).InDocketLine;
			var receive1Line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 20m, locationA1).InDocketLine;
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receive2Line1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 30m).InDocketLine;
			receive2Line1.WE_WL = locationA1.PK;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			var receive3Line1 = Helper.CreateWhsReceiveInventoryLine(receive3, product4, 40m).InDocketLine;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receive2Line1InNewFactory = newFactory.Load<WhsDocketLine>(receive2Line1.PK);
			receive2Line1InNewFactory.WE_OP = product3.PK; // update receive2Line1's product to include prouct 3 in the SOH warning

			var receive3Line1InNewFactory = newFactory.Load<WhsDocketLine>(receive3Line1.PK);
			receive3Line1InNewFactory.WE_WL = locationA1.PK;
			var warning = receive3Line1InNewFactory.WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", string.Format(@"Stock On Hand exists.
Client: {0}, Product: {1}
Client: {0}, Product: {2}
Client: {0}, Product: {3}", data.Org1.OH_Code, receive1Line1.ProductCode, receive1Line2.ProductCode, product3.OP_PartNum), warning);
		}

		public void TestCheckWI_WL_LocationSOH_Dockdoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive1Line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, dockDoorLocation).InDocketLine;
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var receive2Line = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m).InDocketLine;

			// enable SOH location warning
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			receive2Line.WE_WL = dockDoorLocation.PK;
			AssertEquals("Receive line is Received.", InventoryStatus.Codes.Received, receive2Line.WE_OriginalInventoryStatus);
			AssertNoWarnings("Should have no warnings because receive line is not putaway.", receive2Line.WE_WLInfo);
		}
		#endregion

		#region TestCheckWI_WL_LocationCrossDocked

		public void TestCheckWI_WL_LocationCrossDocked()
		{
			var data = new TestDataSimpleEnvironment(Factory)
			{
				Whs1 = Helper.CreateWarehouse("WHS")
			};
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "A", 2, 2);
			Factory.Save();

			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(docket, data.Part1, 10m).InDocketLine;

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var pickLine1 = Helper.CreateReservePickLine(orderLine1, inventoryLine.Inventory[0], 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var pickLine2 = Helper.CreateReservePickLine(orderLine2, inventoryLine.Inventory[0], 3m);

			inventoryLine.WE_WL = row.Locations[0].PK; // "A-1-1"
			AssertHasWarning(inventoryLine.WE_WLInfo, "This receipt line is cross docked (no cross dock location has been selected on the customer order");

			// change
			order1.WD_WL_CrossDock = data.Whs1.FindLocation("A-2-1").PK;
			inventoryLine.WE_WL = row.Locations[1].PK; // "A-1-2"
			AssertHasWarning(inventoryLine.WE_WLInfo, "This receipt line is cross docked, but you have selected a location that is not the customer order's cross dock location");

			order2.WD_WL_CrossDock = data.Whs1.FindLocation("A-2-2").PK;
			inventoryLine.WE_WL = row.Locations[2].PK; // "A-2-1"
			AssertNoWarnings("Should have no warning because Location is in staging area", inventoryLine.WE_WLInfo);
			inventoryLine.WE_WL = row.Locations[3].PK; // "A-2-2"
			AssertNoWarnings("Should have no warning because Location is in staging area", inventoryLine.WE_WLInfo);

			pickLine1.WZ_WE_InventoryLine = ZGuid.Empty;
			inventoryLine.WE_WL = row.Locations[2].PK; // "A-2-1"
			AssertHasWarning(inventoryLine.WE_WLInfo, "This receipt line is cross docked, but you have selected a location that is not the customer order's cross dock location");

			pickLine2.WZ_WE_InventoryLine = ZGuid.Empty;
			inventoryLine.Validation.ValidateWE_WL();
			AssertNoWarnings("Should have no warning because inventory is not cross docked", inventoryLine.WE_WLInfo);
		}

		public void TestCheckWI_WL_LocationCrossDocked_DockdoorLocation_NoCrossDock()
		{
			TestCheckWI_WL_LocationCrossDocked_DockdoorLocation_Core(false, "This receipt line is cross docked (no cross dock location has been selected on the customer order");
		}

		public void TestCheckWI_WL_LocationCrossDocked_DockdoorLocation_WithCrossDock()
		{
			TestCheckWI_WL_LocationCrossDocked_DockdoorLocation_Core(true, "This receipt line is cross docked, but you have selected a location that is not the customer order's cross dock location");
		}

		void TestCheckWI_WL_LocationCrossDocked_DockdoorLocation_Core(bool withCrossDock, string expectedWarningMessage)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			Factory.Save();

			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(docket, data.Part1, 10m).InDocketLine;

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			Helper.CreateReservePickLine(orderLine1, inventoryLine.Inventory[0], 2m);

			if (withCrossDock)
			{
				order1.WD_WL_CrossDock = data.Whs1.FindLocation("A-2").PK; // set cross dock
			}

			inventoryLine.WE_WL = data.Whs1.FindLocation("A-1").PK;
			AssertEquals("Receive line is Putaway.", InventoryStatus.Codes.Putaway, inventoryLine.WE_OriginalInventoryStatus);
			AssertHasWarning("Should have warning since receive line is putaway.", inventoryLine.WE_WLInfo, expectedWarningMessage);

			inventoryLine.WE_WL = dockDoorLocation.PK;
			AssertEquals("Receive line is Received.", InventoryStatus.Codes.Received, inventoryLine.WE_OriginalInventoryStatus);
			AssertNoWarning("Should have no warning since receive line is not putaway.", inventoryLine.WE_WLInfo, expectedWarningMessage);
		}

		public void TestCheckWI_WL_LocationCrossDocked_WorkOrder()
		{
			// create bom product and inventory

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bomProduct = data.Part1;
			Helper.CreateProductBOM(bomProduct, data.Part2, 1m, "UNT");

			// create receive

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 5m, false, false);
			Factory.Save();
			AssertEquals("Precondition - receive should not be finalised", false, receive.IsFinalised);

			// create work order and cross dock

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1");
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			Helper.CreateReservePickLine(workOrderLine.ChildComponentLines.ElementAt(0), receive.Inventory[0], 1m);
			Factory.Save();

			AssertNoExceptionThrown(() => receive.Inventory[0].WI_WL = data.Whs1.DefaultLocation.PK);
		}

		#endregion

		#region TestCheckWI_WL_LocationWarnsIfPendingOrders

		public void TestCheckWI_WL_LocationWarnsIfPendingOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(docket, data.Part1, 10m).InDocketLine;

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part2, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 3m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3");
			Helper.CreateWhsOrderLine(order3, data.Part2, 4m);

			Factory.Save(); // for dbonly query in validation
			var warnMessage = "This product is required by the following pending order(s): 1, 2";

			inventoryLine.WE_WL = data.Whs1.FindLocation("A-1-1").PK; // "A-1-1"
			AssertNoWarning("Should have no warning because order lines are for diff product", inventoryLine.WE_WLInfo, warnMessage);

			orderLine1.WE_OP = data.Part1.PK;
			orderLine2.WE_OP = data.Part1.PK;
			Factory.Save(); // for dbonly query in validation

			inventoryLine.Validation.ValidateWE_WL();
			AssertHasWarning(inventoryLine.WE_WLInfo, warnMessage);

			Helper.CreateReservePickLine(orderLine1, inventoryLine.Inventory[0], 2m);

			Factory.Save(); // for dbonly query in validation

			inventoryLine.Validation.ValidateWE_WL();
			AssertNoWarning("Should have no warning because inventory is cross docked", inventoryLine.WE_WLInfo, warnMessage);
		}

		public void TestCheckWI_WL_LocationWarnsIfPendingOrders_WithReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(docket, data.Part1, 10m).InDocketLine;

			var docket2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(docket2, data.Part1, 10m);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 4m);

			inventoryLine.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			orderLine2.ReserveStockIfAbleTo(inventoryLine2);
			Factory.Save(); // for dbonly query in validation

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(docket.PK);
			var inventoryLineInNewFactory = receiveInNewFactory.Lines.Single();
			receiveInNewFactory.RunPreSaveValidation();
			AssertHasWarning(inventoryLineInNewFactory.WE_WLInfo, "This product is required by the following pending order(s): 1, 2, 3");

			orderLine3.ReserveStockIfAbleTo(inventoryLine2, 2);
			Factory.Save(); // for dbonly query in validation

			receiveInNewFactory.RunPreSaveValidation();
			AssertHasWarning(inventoryLineInNewFactory.WE_WLInfo, "This product is required by the following pending order(s): 1, 2, 3");

			orderLine3.ReserveStockIfAbleTo(inventoryLine2);
			Factory.Save(); // for dbonly query in validation

			receiveInNewFactory.RunPreSaveValidation();
			AssertHasWarning(inventoryLineInNewFactory.WE_WLInfo, "This product is required by the following pending order(s): 1, 3");
		}

		public void TestCheckWI_WL_LocationWarnsIfPendingOrders_MoreThanMaxPendingOrdersToDisplay()
		{
			TestCheckWI_WL_LocationWarnsIfPendingOrders_DisplayPendingOrdersCore(9,
				"This product is required by the following pending order(s): 1, 2, 3, 4, 5, 6, 7, 8, ...");
		}

		public void TestCheckWI_WL_LocationWarnsIfPendingOrders_MaxPendingOrdersToDisplay()
		{
			TestCheckWI_WL_LocationWarnsIfPendingOrders_DisplayPendingOrdersCore(8,
				"This product is required by the following pending order(s): 1, 2, 3, 4, 5, 6, 7, 8");
		}

		void TestCheckWI_WL_LocationWarnsIfPendingOrders_DisplayPendingOrdersCore(int ordersToCreate, string expectedWarningMessage)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			for (var orderCount = 1; orderCount <= ordersToCreate; orderCount++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, orderCount.ToString());
				Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			}

			Factory.Save(); // for dbonly query in validation

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			inventoryLine.WE_WL = data.Whs1.FindLocation("A-1-1").PK; // "A-1-1"
			AssertHasWarning(inventoryLine.WE_WLInfo, expectedWarningMessage);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			var inventoryLineInNewFactory = receiveInNewFactory.Lines.Single();
			receiveInNewFactory.RunPreSaveValidation();
			AssertHasWarning(inventoryLineInNewFactory.WE_WLInfo, expectedWarningMessage);
		}

		[StressTest]
		public void TestCheckWI_WL_LocationWarnsIfPendingOrders_StressTest_ManyLines()
		{
			const int numberOfLinesPerOrder = 33000;

			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var order = new WhsDocketDO(data.Org1.PK.ToGuid(), data.Whs1.PK.ToGuid(), "ORD", "ORD", "ENT", "1").InsertAndReturnObject(TestConnection);
			var orderLineDOList = Enumerable.Range(1, numberOfLinesPerOrder)
				.Select(i => new WhsDocketLineDO(order, data.Part1.PK.ToGuid(), 1))
				.ToArray();
			TestConnection.ExecuteNonQuery(orderLineDOList.GetBulkInsertStatement());

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			inventoryLine.WE_WL = data.Whs1.FindLocation("A-1-1").PK;
			AssertHasWarningContaining(inventoryLine.WE_WLInfo, "This product is required by the following pending order(s):");
		}

		#endregion

		#region TestCheckLocationString_CheckFixLocationMaxProductType

		public void TestCheckLocationString_CheckFixLocationMaxProductType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			var maximumNumberOfProducts = 1;
			var fixLocationType = Helper.CreateLocationType("TE1", "Test 1", false, maximumNumberOfProducts, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("TE2", "Test 2", false, 0, LocationClasses.Codes.NOR);
			var otherClient = Helper.CreateClient();
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var fixedLcoation = locations[0];
			var normalLocation = locations[1];
			fixedLcoation.WLV_WLT_LocationType = fixLocationType.PK;
			normalLocation.WLV_WLT_LocationType = normalLocationType.PK;

			var fixedLcoationName = fixedLcoation.ToLocationString();
			var normalLocationName = normalLocation.ToLocationString();

			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, data.Whs1, fixedLcoationName);
			Factory.Save();

			var expectedErrMessages = "This location is a fixed pick face location and '{0}' is not assigned to this location.";

			AssertEquals(string.Format(expectedErrMessages, data.Part2.OP_PartNum),
				WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(fixedLcoation, data.Org1, data.Part2));
			AssertEquals(string.Format(expectedErrMessages, data.Part1.OP_PartNum),
				WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(fixedLcoation, otherClient, data.Part1));

			AssertEquals("", WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(normalLocation, data.Org1, data.Part1));
			AssertEquals("", WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(normalLocation, data.Org1, data.Part2));
			AssertEquals("", WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(fixedLcoation, data.Org1, data.Part1));
		}

		#endregion

		#region TestCheckJobHasRecordWithSameProductCodeButDifferentPropertyValue

		public void TestCheckJobHasRecordWithSameProductCodeButDifferentPropertyValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			var receiveLine1 = inventory1.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;
			receiveLine1.WE_OP = ZGuid.Invalid;
			receiveLine2.WE_OP = ZGuid.Invalid;
			receiveLine1.ProductCode = "TEMP1";
			receiveLine2.ProductCode = "TEMP1";
			receiveLine1.ProductDesc = "DESC1";
			receiveLine2.ProductDesc = "ANOTHERDESC2";
			AssertHasError("When we have same product code for temporary products but different description, we should have the error.",
				receiveLine2.ProductDescInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine2.ProductDesc = "DESC1";
			AssertNoError("When we have same product code for temporary products and same descriptions, we should have no errors.",
				receiveLine2.ProductDescInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine2.ProductCode = "TEMP2";
			receiveLine2.ProductDesc = "ANOTHERDESC2";
			AssertNoError("When we have different product code for temporary products and different descriptions, we should have no errors.",
				receiveLine2.ProductDescInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine2.ProductDesc = "DESC1";
			AssertNoError("When we have different product code for temporary products and same descriptions, we should have no errors.",
				receiveLine2.ProductDescInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);

			receiveLine2.WE_OP = data.Part1.PK;
			receiveLine2.ProductCode = "TEMP1";
			receiveLine2.ProductDesc = "ANOTHERDESC2";
			AssertNoError("When the product is not temporary product, we should have no errors.",
				receiveLine2.ProductDescInfo, WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
		}

		#endregion

		#region TestGetIsFixedLocation

		public void TestGetIsFixedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			// setup fixed locations
			var fixLocationType = Helper.CreateLocationType("TE1", "Test 1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("TE2", "Test 2", false, 0, LocationClasses.Codes.NOR);

			locationA1.WLV_WLT_LocationType = fixLocationType.PK;
			locationA2.WLV_WLT_LocationType = normalLocationType.PK;

			AssertEquals(true, WhsValidationHelper.GetIsFixedLocation(locationA1));
			AssertEquals(false, WhsValidationHelper.GetIsFixedLocation(locationA2));
		}

		#endregion

		#region TestGetAvailableAndPendingTotalInventoryQuantity

		public void TestGetAvailableAndPendingTotalInventoryQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, locations[0], "", false, false); //unfinalised
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locations[0], "", false, true); // finalised
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 8m, locations[1], "", false, true); // finalised
			var unFinalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 3m, locations[0], "", false, false); //unfinalised
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, locations[0], locations[1]);
			transfer.RunPreSaveValidation(); // to commit units
			Factory.Save();

			var loc1Inv = Factory.Load<WhsInventoryView>(WhsInventoryFilterBuilder.BuildFilter(
				client: null,
				part: null,
				warehouse: data.Whs1,
				area: null,
				location: locations[0],
				palletID: "",
				bondedEntryKey: "",
				expiryDate: ZDate.Empty,
				packingDate: ZDate.Empty,
				partAttrib1: "",
				partAttrib2: "",
				partAttrib3: "",
				serialNumber: "",
				arrivalDate: ZDateTimeOffset.Empty));
			AssertEquals(10m + 5m + 3m, WhsValidationHelper.GetAvailableAndPendingTotalInventoryQuantity(locations[0], unFinalisedReceive, loc1Inv, true));
			AssertEquals(10m + 5m, WhsValidationHelper.GetAvailableAndPendingTotalInventoryQuantity(locations[0], unFinalisedReceive, loc1Inv, false));

			var loc2Inv = Factory.Load<WhsInventoryView>(WhsInventoryFilterBuilder.BuildFilter(
				client: null,
				part: null,
				warehouse: data.Whs1,
				area: null,
				location: locations[1],
				palletID: "",
				bondedEntryKey: "",
				expiryDate: ZDate.Empty,
				packingDate: ZDate.Empty,
				partAttrib1: "",
				partAttrib2: "",
				partAttrib3: "",
				serialNumber: "",
				arrivalDate: ZDateTimeOffset.Empty));
			AssertEquals(8m + 2m, WhsValidationHelper.GetAvailableAndPendingTotalInventoryQuantity(locations[1], transfer, loc2Inv, true));
			AssertEquals(8m, WhsValidationHelper.GetAvailableAndPendingTotalInventoryQuantity(locations[1], transfer, loc2Inv, false));
		}

		#endregion

		#endregion

		#region TestCheckLocationForMaxWeightVolumeQuantity_ProcessesQuantityThenWeightThenVolume

		public void TestCheckLocationForMaxWeightVolumeQuantity_ProcessesQuantityThenWeightThenVolume()
		{
			// more tests for warnings and errors for the tested method are defined above. See TestCheckWI_WL_... tests
			// We don't invoke CheckLocationForMaxWeightVolumeQuantity here as architecture will prevent us from setting 
			// property error outside Check method of a bizO.

			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			location.WLV_MaxQuantity = 5m;
			location.WLV_MaxCubic = 10m;
			location.WLV_MaxCubicUnit = "L";
			location.WLV_MaxWeight = 20m;
			location.WLV_MaxWeightUnit = "KG";

			data.Part1.OP_Cubic = 3m;
			data.Part1.OP_CubicUQ = "L";
			data.Part1.OP_Weight = 5m;
			data.Part1.OP_WeightUQ = "KG";
			Factory.Save();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m, location, "", false, false);
			var receiveLine = receive.Lines[0];

			AssertHasError(receiveLine.WE_WLInfo, "Total required Quantity (6) exceeds the maximum available Quantity (5.000) for this location.");
			AssertNoWarnings(receiveLine.WE_WLInfo);

			location.WLV_MaxQuantity = 0;
			receiveLine.Validation.ValidateWE_WL();
			AssertNoErrors(receiveLine.WE_WLInfo);
			AssertHasWarning(receiveLine.WE_WLInfo, "Total required Weight (30.00 KG) exceeds the maximum available Weight (20.00 KG) for this location.");

			location.WLV_MaxWeight = 0;
			receiveLine.Validation.ValidateWE_WL();
			AssertNoErrors(receiveLine.WE_WLInfo);
			AssertHasWarning(receiveLine.WE_WLInfo, "Total required Volume (18.000 L) exceeds the maximum available Volume (10.000 L) for this location.");

			location.WLV_MaxCubic = 0;
			receiveLine.Validation.ValidateWE_WL();
			AssertNoErrors(receiveLine.WE_WLInfo);
			AssertNoWarnings(receiveLine.WE_WLInfo);
		}

		#endregion

		#region TestLocationCapacityCheckIsNotIgnoredInRF

		public void TestLocationCapacityCheckIsNotIgnoredInRF()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");
			location.WLV_MaxQuantity = 1m;
			Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location, "", false, false);
				AssertHasError(receive.Lines[0].WE_WLInfo, "The Maximum Qty(1) for the destination location will be exceeded. Please select another location.");
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		#endregion

		#region TestGetLocationAvailableCapacityNotDependsOnLocationType

		public void TestGetLocationAvailableCapacityNotDependsOnLocationType_Normal()
		{
			GetLocationAvailableCapacityNotDependsOnLocationType_Core(LocationStatus.Codes.Normal);
		}

		public void TestGetLocationAvailableCapacityNotDependsOnLocationType_Damaged()
		{
			GetLocationAvailableCapacityNotDependsOnLocationType_Core(LocationStatus.Codes.Damaged);
		}

		public void TestGetLocationAvailableCapacityNotDependsOnLocationType_Held()
		{
			GetLocationAvailableCapacityNotDependsOnLocationType_Core(LocationStatus.Codes.Held);
		}

		void GetLocationAvailableCapacityNotDependsOnLocationType_Core(string locationStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;
			Helper.SetLocationMaxWeightAndVolume(location, 20m, Constants.Weight.Kilograms, 12m, Constants.Volume.CubicMetres);
			location.WLV_MaxQuantity = 10;
			location.WLV_LocationStatus = locationStatus;
			Factory.Save();

			var docket = Factory.New<WhsReceive>();
			var availableCapacity = docket.LocationCapacityValidationManager.GetLocationAvailableCapacity(location);
			AssertEquals(10m, availableCapacity.Quantity);
			AssertEquals(20m, availableCapacity.Weight);
			AssertEquals(12m, availableCapacity.Volume);
		}

		#endregion

		#region TestGetTotalInventoryStorage

		public void TestGetTotalInventoryStorage()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateProductUnit(data.Part1, "G", "KG", 1000m);
			Helper.CreateProductUnit(data.Part1, "L", "M3", 1000m);
			data.Part1.OP_PartNum = "WATER";
			data.Part1.OP_Cubic = 1m;
			data.Part1.OP_CubicUQ = "L";
			data.Part1.OP_Weight = 1m;
			data.Part1.OP_WeightUQ = "KG";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, finalise: false);
			var inventory1 = receive1.Inventory[0];
			inventory1.WI_InDocketLineUnits = 3m;
			inventory1.WI_TotalUnits = 1m;
			Assert("Precondition:", !receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1m);
			var inventory2 = receive2.Inventory[0];
			inventory2.WI_InDocketLineUnits = 11m;
			inventory2.WI_TotalUnits = 5m;
			AssertIsFinalisedPrecondition(receive2);

			AssertEquals("Total Qty should 3 + 5 = 8, and converted to G it will be 8000 G", 8000m, WhsValidationHelper.GetTotalInventoryStorage(new[] { inventory1, inventory2 }, "G", OrgSupplierPartSchema.OP_Weight, OrgSupplierPartSchema.OP_WeightUQ));
			AssertEquals("Total Qty should 3 + 5 = 8, and converted to G it will be 0.008 M3", 0.008m, WhsValidationHelper.GetTotalInventoryStorage(new[] { inventory1, inventory2 }, "M3", OrgSupplierPartSchema.OP_Cubic, OrgSupplierPartSchema.OP_CubicUQ));
		}

		#endregion

		#region TestGetPendingUnitsIntoLocaion

		public void TestGetPendingUnitsIntoLocaion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, locationA1, "");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 3m, locationA1, "");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 5m, locationA1, "");

			Factory.Save();

			AssertEquals(0m, WhsValidationHelper.GetPendingUnitsIntoLocaion(receive1, locationA2.PK));
			AssertEquals(0m, WhsValidationHelper.GetPendingUnitsIntoLocaion(receive1, ZGuid.Invalid));
			AssertEquals(0m, WhsValidationHelper.GetPendingUnitsIntoLocaion(null, locationA1.PK));

			AssertEquals("Should sum units from receive2 and receive3", 8m, WhsValidationHelper.GetPendingUnitsIntoLocaion(receive1, locationA1.PK));
			AssertEquals("Should sum units from receive1 and receive3", 6m, WhsValidationHelper.GetPendingUnitsIntoLocaion(receive2, locationA1.PK));
			AssertEquals("Should sum units from receive1 and receive2", 4m, WhsValidationHelper.GetPendingUnitsIntoLocaion(receive3, locationA1.PK));
		}

		#endregion

		#region TestGetPendingDocketLinesAtLocation

		public void TestGetPendingDocketLinesAtLocation()
		{
			AssertExceptionThrown<ArgumentNullException>("Should have an exception when docket is not provided", () => WhsValidationHelper.GetPendingDocketLinesAtLocation(null, ZGuid.NewZGuid()));

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "").InDocketLine;

			var anotherNotFinalisedReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var notFinalisedReceiveLineToA1 = Helper.CreateWhsReceiveInventoryLine(anotherNotFinalisedReceive, data.Part1, 1m, locationA1, "").InDocketLine;
			var notFinalisedReceiveLineToA2 = Helper.CreateWhsReceiveInventoryLine(anotherNotFinalisedReceive, data.Part1, 1m, locationA2, "").InDocketLine;
			var notFinalisedReceiveLineToA1WithZeroUnits = Helper.CreateWhsReceiveInventoryLine(anotherNotFinalisedReceive, data.Part1, 0m, locationA1, "").InDocketLine;

			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1m, locationA1, "");
			var finalisedReceiveLine = finalisedReceive.Lines.Single();

			Factory.Save();

			var pendingDocketLines = WhsValidationHelper.GetPendingDocketLinesAtLocation(receive, locationA1.PK);
			Assert("receiveLine should not be returned as it is in the docket we are excluding", !pendingDocketLines.Contains(receiveLine));
			Assert("notFinalisedReceiveLineToA1 should be returned as it is not finalised, from other docket, for the target location and WE_TransactionQuantity > 0", pendingDocketLines.Contains(notFinalisedReceiveLineToA1));
			Assert("receiveLine should not be returned as it is for different location", !pendingDocketLines.Contains(notFinalisedReceiveLineToA2));
			Assert("receiveLine should not be returned as it has WE_TransactionQuantity = 0", !pendingDocketLines.Contains(notFinalisedReceiveLineToA1WithZeroUnits));
			Assert("receiveLine should not be returned as it is finalised", !pendingDocketLines.Contains(finalisedReceiveLine));
			AssertEquals(1, pendingDocketLines.Length);

			AssertEquals("There should be no error if we pass empty location PK. But the results should be empty as well.", 0, WhsValidationHelper.GetPendingDocketLinesAtLocation(receive, ZGuid.Empty).Length);
			AssertEquals("There should be no error if we pass invalid location PK. But the results should be empty as well.", 0, WhsValidationHelper.GetPendingDocketLinesAtLocation(receive, ZGuid.Invalid).Length);
		}

		#endregion

		#region TestGetTotalInventoryQuantity

		public void TestGetTotalInventoryQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.FindLocation("A"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 3m, data.Whs1.FindLocation("A"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5m, data.Whs1.FindLocation("A"));
			Assert("Precondition: not finalised", !receive2.IsFinalised);

			AssertEquals("Only finalised inventories should be counted (1 + 3 = 4).", 4m, WhsValidationHelper.GetTotalInventoryQuantity(new[] { inventory1, inventory2, inventory3 }));
		}

		#endregion
	}
}
