using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveLine))]
	public class WhsReceiveLineTest : WhsDocketLineTestCase<WhsReceiveLine, WhsReceive>
	{
		#region Related Entities

		#region TestPickLines

		public void TestGetPartAttributeReadonly_WithNullProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 0);
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			receiveLine.WE_OP = ZGuid.Empty;
			var info = receiveLine.WE_ExpiryDateInfo;
			receiveLine.WE_ExpiryDate = ZDate.BrettsBirthday;

			AssertEquals(true, info.ReadOnly);
		}

		public void TestPickLines()
		{
			// pick lines collection on receive line should have no relationship to the line
			var receiveLine = Factory.New<WhsReceiveLine>();
			AssertNull(receiveLine.PickLines.Relationship.Master);
		}

		#endregion

		#region TestReceiveLineValidationStrategy

		public void TestReceiveLineValidationStrategy()
		{
			var inventoryLine1 = Factory.New<WhsReceiveLine>();
			AssertEquals("When run by Enterprise", typeof(WhsReceiveLineValidationStrategy), inventoryLine1.ReceiveLineValidationStrategy.GetType());

			try
			{
				Globals.IsWeb = true;
				var inventoryLine2 = Factory.New<WhsReceiveLine>();
				AssertEquals("When run by WebTracker", typeof(WhsReceiveLineValidationStrategy), inventoryLine2.ReceiveLineValidationStrategy.GetType());

				Globals.IsUserInteractive = false;
				var inventoryLine3 = Factory.New<WhsReceiveLine>();
				AssertEquals("When run by RF web service", typeof(WhsReceiveLineValidationRFStrategy), inventoryLine3.ReceiveLineValidationStrategy.GetType());

				Globals.IsWeb = false;
				var inventoryLine4 = Factory.New<WhsReceiveLine>();
				AssertEquals("When run by Service Tasks", typeof(WhsReceiveLineValidationStrategy), inventoryLine4.ReceiveLineValidationStrategy.GetType());
			}
			finally // clean up
			{
				Globals.IsWeb = false;
				Globals.IsUserInteractive = true;
			}
		}

		#endregion

		#region BOMComponentLinksForBinding

		public void TestBOMComponentLinksForBinding()
		{
			var receiveLine1 = Factory.New<WhsReceiveLine>();
			AssertEquals(0, receiveLine1.BOMComponentLinksForBinding.Count);

			var receiveLine2 = Factory.New<WhsReceiveLine>();
			var link1 = Factory.New<WhsBOMInventoryPivot>();
			var link2 = Factory.New<WhsBOMInventoryPivot>();
			var link3 = Factory.New<WhsBOMInventoryPivot>();
			link1.WIP_WE_InventoryLine = receiveLine2.PK;
			link2.WIP_WE_InventoryLine = receiveLine2.PK;
			link3.WIP_WE_InventoryLine = receiveLine1.PK;
			var workOrderLine1 = Factory.New<WhsWorkOrderLine>();
			var workOrderLine2 = Factory.New<WhsWorkOrderLine>();
			var workOrderLine3 = Factory.New<WhsWorkOrderLine>();
			var inventoryLine1 = Factory.New<WhsReceiveLine>();
			var inventoryLine2 = Factory.New<WhsReceiveLine>();
			var pickLine1 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_InventoryLine = inventoryLine1.PK;
			pickLine1.WZ_WE_TransactionLine = workOrderLine1.PK;
			pickLine1.WZ_Units = 1m;
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine2.WZ_WE_InventoryLine = inventoryLine2.PK;
			pickLine2.WZ_WE_TransactionLine = workOrderLine1.PK;
			pickLine2.WZ_Units = 1m;
			var pickLine3 = Factory.New<WhsPickLine>();
			pickLine3.WZ_WE_TransactionLine = workOrderLine2.PK;
			pickLine3.WZ_Units = 1m;
			pickLine3.WZ_WE_OriginalPickedInventoryLine = inventoryLine1.PK;
			var pickLine4 = Factory.New<WhsPickLine>();
			pickLine4.WZ_WE_InventoryLine = inventoryLine1.PK;
			pickLine4.WZ_WE_TransactionLine = workOrderLine3.PK;
			pickLine4.WZ_Units = 1m;
			link1.WIP_WE_ComponentLine = workOrderLine1.PK;
			link2.WIP_WE_ComponentLine = workOrderLine2.PK;
			link3.WIP_WE_ComponentLine = workOrderLine3.PK;
			AssertContainsExactElementsInAnyOrder(new[] { inventoryLine1, inventoryLine2 }, receiveLine2.BOMComponentLinksForBinding.Select(pl => pl.InventoryLine));
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 2m, 1m }, receiveLine2.BOMComponentLinksForBinding.Select(pl => pl.UnitsToPick));
			AssertEquals(false, receiveLine2.BOMComponentLinksForBinding.AllowNew);
			AssertEquals(false, receiveLine2.BOMComponentLinksForBinding.AllowRemove);
		}

		#endregion

		#endregion

		#region Business Object Overrides

		#region FetchStrategy

		protected override Type FetchStrategyType
		{
			get { return typeof(WhsReceiveLineFetchStrategy); }
		}

		#endregion

		#region TestClone

		public void TestClone_TempProduct()
		{
			var receiveLine = DocketLine;
			receiveLine.WE_OP = ZGuid.Invalid;
			receiveLine.ProductCode = "TempProduct";
			receiveLine.ProductDesc = "TempDesc";
			receiveLine.CommodityCode = "TEMP";
			receiveLine.ProductUQ = "XXX";
			AssertEquals("Precondition", ZGuid.Invalid, receiveLine.WE_OP);
			AssertEquals("Precondition", "TempProduct", receiveLine.ProductCode);
			AssertEquals("Precondition", "TempDesc", receiveLine.ProductDesc);
			AssertEquals("Precondition", "TEMP", receiveLine.CommodityCode);
			AssertEquals("Precondition", "XXX", receiveLine.ProductUQ);

			var clone = (WhsReceiveLine)receiveLine.Clone();
			AssertEquals("The property should be cloned.", ZGuid.Invalid, clone.WE_OP);
			AssertEquals("The property should be cloned.", "TempProduct", clone.ProductCode);
			AssertEquals("The property should be cloned.", "TempDesc", clone.ProductDesc);
			AssertEquals("The property should be cloned.", "TEMP", clone.CommodityCode);
			AssertEquals("The property should be cloned.", "XXX", clone.ProductUQ);
		}

		public void TestCloneWE_WE_OriginalDocketLineForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation).InDocketLine;
			Factory.Save();

			var clone = (WhsReceiveLine)receiveLine.Clone();
			receive.Lines.Add(clone);
			Assert("Precondition:", receiveLine.WE_IsOriginalInventory);
			AssertEquals(clone.PK, clone.WE_WE_OriginalDocketLineForRating);
			AssertNotEquals(receiveLine.WE_WE_OriginalDocketLineForRating, clone.WE_WE_OriginalDocketLineForRating);

			receive.Lines.Delete(clone);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 6;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var statusChangeLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.PK, SQLComparisonOperator.NotEqual, receiveLine.PK));
			Assert("Precondition:", !statusChangeLine.WE_IsOriginalInventory);
			var cloneStatusChangeLine = (WhsReceiveLine)statusChangeLine.Clone();
			Assert("Precondition:", !cloneStatusChangeLine.WE_IsOriginalInventory);
			AssertEquals("For cloned status changed inventory WE_WE_OriginalDocketLineForRating should still point to original inventory line.", receiveLine.PK, cloneStatusChangeLine.WE_WE_OriginalDocketLineForRating);
			AssertNotEquals(cloneStatusChangeLine.PK, cloneStatusChangeLine.WE_WE_OriginalDocketLineForRating);
		}

		public void TestClone_Consignee()
		{
			var receiveLine = DocketLine;
			var consignee = Helper.CreateClient();
			receiveLine.ConsigneeNameOrPK = consignee.PK.ToString();

			var clone = (WhsReceiveLine)receiveLine.Clone();
			AssertEquals("ConsigneeNameOrPK", consignee.PK.ToString(), clone.ConsigneeNameOrPK);
		}

		public void TestClone_WE_ClientOrderedUnits_BeforeStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation).InDocketLine;
			AssertEquals("Precondition.", 10m, receiveLine.WE_ClientOrderedUnits);
			Factory.Save();

			var clone = (WhsReceiveLine)receiveLine.Clone();
			receive.Lines.Add(clone);
			Assert("Precondition:", receiveLine.WE_IsOriginalInventory);
			AssertEquals("Should have cloned client ordered units.", 10m, clone.WE_ClientOrderedUnits);
			AssertEquals("Should have cloned transaction quantity.", 10m, clone.WE_TransactionQuantity);
		}

		public void TestClone_WE_ClientOrderedUnits_AfterStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation).InDocketLine;
			AssertEquals("Precondition.", 10m, receiveLine.WE_ClientOrderedUnits);
			Factory.Save();

			var clone = (WhsReceiveLine)receiveLine.Clone();
			receive.Lines.Add(clone);
			Assert("Precondition:", receiveLine.WE_IsOriginalInventory);
			AssertEquals("Should *not* have cloned client ordered units.", 0m, clone.WE_ClientOrderedUnits);
			AssertEquals("Should have cloned transaction quantity.", 10m, clone.WE_TransactionQuantity);
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues_CreatesInventory()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			AssertEquals("When new receive line is being created it should create an Inventory right away.", 1, receiveLine.Inventory.Count);
			AssertEquals("Inventories original docket line should be the receive line that created it.", receiveLine.PK, receiveLine.Inventory[0].WI_WE_OriginalInDocketLineForRating);
			AssertEquals("Inventory should be linked to receive docket.", DocketType.Codes.Receive, receiveLine.Inventory[0].WI_InDocketLineType);
		}

		#endregion

		#region TestIsInventoryLine

		protected override void TestIsInventoryLineCore()
		{
			AssertEquals(true, DocketLine.IsInventoryLine);
		}

		#endregion

		#endregion

		#region Validation

		#region TestValidationType

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsReceiveLineValidation);
		}

		#endregion

		#region TestValidationUS

		public void TestValidationUS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save(); // save to create receive line
			AssertEquals(typeof(US.WhsReceiveLineValidationUS), receive.Lines[0].Validation.GetType());
		}

		#endregion

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsReceiveLineLookups);
		}

		#endregion

		#region Properties

		#region TestHeldCodeToChangeTo_ConsidersStockAlreadyReserved

		public void TestHeldCodeToChangeTo_ConsidersStockAlreadyReserved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			orderLine.ReserveStockIfAbleTo(receiveLine.Inventory[0]);
			AssertEquals("Precondition", 4m, receiveLine.Inventory[0].WI_AvailableToPickQuantity);
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertEquals(4m, receiveLine.HeldCodeChangeQuantity);
			AssertNoErrors(receiveLine.HeldCodeChangeQuantityInfo);
			AssertNoErrors(receiveLine.HeldCodeToChangeToInfo);

			receiveLine.HeldCodeChangeQuantity = 9m;
			receiveLine.IsInventoryEditForm = true;
			Factory.Save();

			var newDocketLine = Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 4m));
			AssertEquals(InventoryStatus.Codes.Held, newDocketLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region WE_TransactionQuantity

		protected override bool SettingWE_TransactionQuantityUpdatesTotalLineUnits
		{
			get { return false; }
		}

		#region TestWE_TransactionQuantity_SetValueToInventoryView

		public void TestWE_TransactionQuantity_SetValueToInventoryView()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", 0m, docketLine.WE_TransactionQuantity);

			TestSetValueToInventoryView(docketLine, WhsDocketLineSchema.WE_TransactionQuantity.Name, WhsInventoryViewSchema.WI_InDocketLineUnits.Name, (ZDecimal)2m, (ZDecimal)5m, (ZDecimal)7m);
		}

		#endregion

		#region TestWE_TransactionQuantity_DoesNotSetClientOrderedUnits

		public void TestWE_TransactionQuantity_DoesNotSetClientOrderedUnits()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", 0m, docketLine.WE_TransactionQuantity);
			AssertEquals("Precondition", 0m, docketLine.WE_ClientOrderedUnits);

			docketLine.WE_TransactionQuantity = 10m;
			AssertEquals("WE_TransactionQuantity is assigned.", 10m, docketLine.WE_TransactionQuantity);
			AssertEquals("WE_ClientOrderedUnits remains unassigned.", 0m, docketLine.WE_ClientOrderedUnits);
		}

		#endregion

		#endregion

		#region WE_ClientOrderedUnits

		public void TestWE_ClientOrderedUnits_SetsTransactionQuantity()
		{
			var docketLine = DocketLine;
			AssertEquals("Precondition", 0m, docketLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition", 0m, docketLine.WE_TransactionQuantity);

			docketLine.WE_ClientOrderedUnits = 10m;
			AssertEquals("WE_ClientOrderedUnits is assigned.", 10m, docketLine.WE_ClientOrderedUnits);
			AssertEquals("WE_TransactionQuantity is assigned.", 10m, docketLine.WE_TransactionQuantity);
		}

		public void TestWE_ClientOrderedUnits_FinalisedReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Precondition: WE_ClientOrderedUnits is assigned.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: WE_TransactionQuantity is assigned.", 10m, receiveLine.WE_TransactionQuantity);
			receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receiveLine);
			receiveLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("WE_ClientOrderedUnits is updated.", 15m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("WE_TransactionQuantity is not updated.", 10m, receiveLine.WE_TransactionQuantity);
		}

		public void TestWE_ClientOrderedUnits_AfterAsnLinesCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Precondition: WE_ClientOrderedUnits is assigned.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: WE_TransactionQuantity is assigned.", 10m, receiveLine.WE_TransactionQuantity);
			Factory.Save();

			receive.PopulateASNLines();
			Factory.Save();

			AssertEquals("Precondition: receive has ASN lines.", true, receive.AsnLines.Count > 0);
			receiveLine.WE_ClientOrderedUnits = 15m;
			AssertEquals("WE_ClientOrderedUnits is updated.", 15m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("WE_TransactionQuantity is not updated.", 10m, receiveLine.WE_TransactionQuantity);
		}

		#endregion

		#region TestWE_PackQuantity

		public void TestWE_PackQuantity_AfterStartedReceiving()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m

			var docketLine = DocketLine;
			var docket = docketLine.Docket;
			docketLine.WE_OP = part.PK;
			docket.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Precondition", true, docket.StartedReceiving);
			AssertEquals("Precondition", 0m, docketLine.WE_PackQuantity);
			AssertEquals("Precondition", 0m, docketLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition", 0m, docketLine.WE_TransactionQuantity);

			docketLine.WE_PackQuantity = 25m;
			AssertEquals(25m, docketLine.WE_TransactionQuantity);
			AssertEquals(0m, docketLine.WE_ClientOrderedUnits);

			docketLine.WE_F3_NKPackType = "CTN";
			AssertEquals(25m, docketLine.WE_PackQuantity);
			AssertEquals(300m, docketLine.WE_TransactionQuantity);
			AssertEquals(0m, docketLine.WE_ClientOrderedUnits);
		}

		public void TestWE_PackQuantity_BeforeStartedReceiving()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1"); // auto creates a PartUnit CTN 12m

			var docketLine = DocketLine;
			docketLine.WE_OP = part.PK;
			AssertEquals("Precondition", false, docketLine.Docket.StartedReceiving);
			AssertEquals("Precondition", 0m, docketLine.WE_PackQuantity);
			AssertEquals("Precondition", 0m, docketLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition", 0m, docketLine.WE_TransactionQuantity);

			docketLine.WE_PackQuantity = 25m;
			AssertEquals(25m, docketLine.WE_ClientOrderedUnits);
			AssertEquals(25m, docketLine.WE_TransactionQuantity);

			docketLine.WE_F3_NKPackType = "CTN";
			AssertEquals(25m, docketLine.WE_PackQuantity);
			AssertEquals(300m, docketLine.WE_ClientOrderedUnits);
			AssertEquals(300m, docketLine.WE_TransactionQuantity);
		}

		#endregion

		#region Calculated Properties

		#region TestOriginalHoldReason

		public void TestOriginalHoldReason()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Hold reason should be empty.", "", receiveLine.OriginalHoldReason);

			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receiveLine.OriginalHoldReason = "Whatever";
			AssertEquals("Orginal Hold reason should be set.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold reason should be set.", "Whatever", receiveLine.WE_CurrentHoldReason);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Original Hold reason should remain set.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Curremt Hold reason should be set.", "Whatever", receiveLine.WE_CurrentHoldReason);

			AssertExceptionThrown(typeof(InvalidOperationException),
				"This property is only used to set the Hold Reason before Finalization or Putaway.", () => receiveLine.OriginalHoldReason = "Something");

			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = receiveLine.WE_StockOnHand;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertEquals("Original Hold reason should retain its value.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold reason should be empty.", "", receiveLine.WE_CurrentHoldReason);
		}

		public void TestOriginalHoldReason_ReceiveLineInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Hold reason should be empty.", "", receiveLine.OriginalHoldReason);

			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receiveLine.OriginalHoldReason = "Whatever";
			AssertEquals("Orginal Hold reason should be set.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold reason should be set.", "Whatever", receiveLine.WE_CurrentHoldReason);

			Factory.Save();

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Original Hold reason should remain set.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Curremt Hold reason should be set.", "Whatever", receiveLine.WE_CurrentHoldReason);

			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = receiveLine.WE_StockOnHand;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertEquals("Original Hold reason should retain its value.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold reason should be empty.", "", receiveLine.WE_CurrentHoldReason);
		}

		public void TestOriginalHoldReason_WhenReceiveLineIsPickedForUnload()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-123");
			AssertEquals("Hold reason should be empty.", "", receiveLine.OriginalHoldReason);

			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receiveLine.OriginalHoldReason = "Whatever";
			AssertEquals("Orginal Hold reason should be set.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold reason should be set.", "Whatever", receiveLine.WE_CurrentHoldReason);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT-123", 10m);
			transferLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Receive Line should be picked.", true, receiveLine.IsPickedForUnload);

			AssertExceptionThrown(typeof(InvalidOperationException),
				"This property is only used to set the Hold Reason before Finalization or Putaway.", () => receiveLine.OriginalHoldReason = "Something");
		}

		#endregion

		#region TestSplitQuantity

		public void TestSplitQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, allocateLocations: false, finalise: false);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			receiveLine.Inventory[0].WI_SplitQuantity = 10m;
			AssertEquals(10m, receiveLine.SplitQuantity);

			receiveLine.SplitQuantity = 19m;
			AssertEquals(19m, receiveLine.SplitQuantity);
			AssertEquals(19m, receiveLine.Inventory[0].WI_SplitQuantity);
		}

		#endregion

		#region TestConsignee

		#region TestConsigneeDocAddress

		public void TestConsigneeDocAddress()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var receiveLine = (WhsReceiveLine)data.Line111.InDocketLine;

			Factory.Save(); // To generate DocketLine.

			AssertNotNull("ConsigneeDocAddress should never be null", receiveLine.ConsigneeDocAddress);
			AssertEquals("If DocketLine exist, then ConsigneeDocAddress shouldn't be readonly", false, receiveLine.ConsigneeDocAddress.ReadOnly);

			data.Receive11.FinaliseDocket();
			receiveLine.ConsigneeDocAddress.RefreshBindingIncludingChildren();
			AssertIsFinalisedPrecondition(data.Receive11);
			AssertEquals("ConsigneeDocAddress should be readonly if the Receive is finalised", true, receiveLine.ConsigneeDocAddress.ReadOnly);
		}

		public void TestConsigneeDocAddress_ReadOnlyIsLazyTriggered()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			AssertNotNull("Poke & Precondition", receiveLine.ConsigneeDocAddress);

			AssertPersistentPropertiesHitCount("Getting ConsigneeDocAddress should not trigger property hits.", 0, () => _ = receiveLine.ConsigneeDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = receiveLine.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		#endregion

		#region TestConsigneeDocAddress_AddAdditionalValidation

		public void TestConsigneeDocAddress_AddAdditionalValidation()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;

			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = docket.Lines.AddNew();
			var jobDocAddress = docketLine.ConsigneeDocAddress;
			AssertEquals("Precondition - ConsigneeNameOrPKInfo should have no errors", false, docketLine.ConsigneeNameOrPKInfo.HasErrors());

			jobDocAddress.OrganisationPK = ZGuid.Invalid;
			AssertEquals("ConsigneeNameOrPKInfo should get errors from ConsigneeDocAddress.OrganisationPK", true, docketLine.ConsigneeNameOrPKInfo.HasErrors());
			jobDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("ConsigneeNameOrPKInfo should have no errors", false, docketLine.ConsigneeNameOrPKInfo.HasErrors());

			jobDocAddress.E2_OA_Address = ZGuid.Invalid;
			AssertEquals("ConsigneeNameOrPKInfo should get errors from ConsigneeDocAddress.E2_OA_Address", true, docketLine.ConsigneeNameOrPKInfo.HasErrors());
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("ConsigneeNameOrPKInfo should have no errors", false, docketLine.ConsigneeNameOrPKInfo.HasErrors());

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "1";
			jobDocAddress.E2_Address1 = "1";
			jobDocAddress.E2_Postcode = "1";
			jobDocAddress.E2_City = "1";
			jobDocAddress.E2_State = "NSW";

			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_CompanyNameInfo, "", "OVERRIDDEN");
			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_Address1Info, "", "OVERRIDDEN");
			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_RN_NKCountryCodeInfo, "XX", "AU"); // should be changed to any wrong country code if the XX code was added to the system.
			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_PostcodeInfo, "", "OVERRIDDEN");
			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_CityInfo, "", "OVERRIDDEN");
			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_StateInfo, "", "ACT");
			AssertConsigneeValidationWhenOverridden(docketLine, jobDocAddress.E2_EmailInfo, "AAA", "AAA@AAA.AA");
		}

		void AssertConsigneeValidationWhenOverridden(WhsReceiveLine docketLine, ZPropertyInfo info, ZString invalidValue, ZString validValue)
		{
			info.Value = invalidValue;
			AssertEquals("ConsigneeNameOrPK should get errors from ConsigneeDocAddress." + info.Description + ".", true, docketLine.ConsigneeNameOrPKInfo.HasErrors());

			info.Value = validValue;
			AssertEquals("ConsigneeNameOrPK should have no errors", false, docketLine.ConsigneeNameOrPKInfo.HasErrors());
		}

		#endregion

		#region TestConsignee

		public void TestConsignee()
		{
			var cons1 = Factory.New<OrgHeader>();
			var cons2 = Factory.New<OrgHeader>();

			var docketLine = GetNewBusinessObject();
			docketLine.ConsigneePK = cons1.PK;
			AssertEquals(cons1.PK, docketLine.ConsigneePK);
			AssertEquals(cons1, docketLine.Consignee);

			docketLine.ConsigneePK = cons2.PK;
			AssertEquals(cons2.PK, docketLine.ConsigneePK);
			AssertEquals(cons2, docketLine.Consignee);
		}

		public void TestConsignee_NoDocAddress_DoesNotCreateDocAddress()
		{
			var docketLine = GetNewBusinessObject();

			var docAddressQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, docketLine.PK) { FetchOnlyFromLocalCache = true };
			AssertNull("Precondition: No addresses.", Factory.LoadTop1<JobDocAddress>(docAddressQuery));

			AssertEquals("Should be no consignee.", ZGuid.Empty, docketLine.ConsigneePK);
			AssertNull("Should be no consignee.", docketLine.Consignee);
			AssertNull("Should not have created any addresses.", Factory.LoadTop1<JobDocAddress>(docAddressQuery));
		}

		#endregion

		#region TestConsigneeDocAddressRequirement

		public virtual void TestConsigneeDocAddressRequirement()
		{
			AssertNotNull("ConsigneeDocAddress requirement should exist.", DocketLine.ConsigneeDocAddressRequirement);
			AssertEquals("Invalid Consignee DocAddress requirement", DocAddressType.ConsigneeAddress, DocketLine.ConsigneeDocAddressRequirement.DefaultDocAddressType);
			AssertEquals("Invalid Consignee DocAddress requirement", ContactType.Consignee, DocketLine.ConsigneeDocAddressRequirement.DefaultContactType);

			IDocAddresses iDocket = DocketLine;
			JobDocAddressRequirement requirement = iDocket.GetDocAddressRequirement(DocAddressType.ConsigneeAddress);
			AssertCodeInArray(iDocket.SupportedAddressTypes, DocAddressType.ConsigneeAddress);
			AssertEquals(DocAddressType.ConsigneeAddress, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.Consignee, requirement.DefaultContactType);
		}

		void AssertCodeInArray(IReadOnlyList<DocAddressType> readOnlyList, DocAddressType code)
		{
			AssertEquals("Code should have been found: " + DocAddressTypes.GetCode(Factory, code), true, readOnlyList.Contains(code));
		}

		#endregion

		#region TestConsigneeNameOrPK

		public void TestConsigneeNameOrPK()
		{
			var cons1 = Factory.NewWithValidTestData<OrgHeader>();
			var cons2 = Factory.NewWithValidTestData<OrgHeader>();

			// ConsigneeDocAddress setup affect ConsigneeNameOrPK
			DocketLine.ConsigneeDocAddress.OrganisationPK = cons1.PK;
			AssertEquals(cons1.PK.ToString(), DocketLine.ConsigneeNameOrPK);

			DocketLine.ConsigneeDocAddress.E2_AddressOverride = true;
			DocketLine.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN NAME";
			AssertEquals("OVERRIDEN NAME", DocketLine.ConsigneeNameOrPK);
			DocketLine.ConsigneeDocAddress.E2_AddressOverride = false; // clear up the data.

			// ConsigneeNameOrPK setup affect ConsigneeDocAddress 
			DocketLine.ConsigneeNameOrPK = cons2.PK.ToString();
			AssertEquals(cons2.PK, DocketLine.ConsigneeDocAddress.OrganisationPK);

			DocketLine.ConsigneeDocAddress.E2_AddressOverride = true;
			DocketLine.ConsigneeNameOrPK = "OVERRIDEN NAME";
			AssertEquals("OVERRIDEN NAME", DocketLine.ConsigneeDocAddress.E2_CompanyName);
		}

		public void TestConsigneeNameOrPK_MaxLength()
		{
			DocketLine.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(JobDocAddressSchema.E2_CompanyName.MaxLength, DocketLine.ConsigneeNameOrPKInfo.MaxLength);

			DocketLine.ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals(ZGuid.Empty.ToString().Length, DocketLine.ConsigneeNameOrPKInfo.MaxLength);
		}

		#endregion

		#region TestConsigneeFieldType

		public void TestConsigneeFieldType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var receiveLine = receive.Lines[0];

			receiveLine.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), receiveLine.ConsigneeFieldType);

			receiveLine.ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.Guid), receiveLine.ConsigneeFieldType);
		}

		#endregion

		#region TestShouldValidateConsignee

		public void TestShouldValidateConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine = (WhsReceiveLine)Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;
			AssertEquals(false, inventoryLine.ShouldValidateConsignee);

			var consignee = inventoryLine.ConsigneeDocAddress;
			AssertEquals(false, inventoryLine.ShouldValidateConsignee);

			consignee.E2_AddressOverride = true;
			AssertEquals(true, inventoryLine.ShouldValidateConsignee);

			consignee.E2_AddressOverride = false;
			AssertEquals(false, inventoryLine.ShouldValidateConsignee);

			consignee.OrganisationPK = data.Org1.PK;
			AssertEquals(true, inventoryLine.ShouldValidateConsignee);

			consignee.OrganisationPK = ZGuid.Invalid;
			AssertEquals(true, inventoryLine.ShouldValidateConsignee);

			consignee.OrganisationPK = ZGuid.Empty;
			AssertEquals(false, inventoryLine.ShouldValidateConsignee);
		}

		#endregion

		#region TestHumanReadableShortcutName

		protected override void TestHumanReadableShortcutNameCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org = Helper.CreateClient("ABC");
			var product = Helper.CreateProduct("TESTPROD", org);
			var receive = Helper.CreateWhsReceive(org, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, product, 0);
			receiveLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(2022, 12, 12);

			AssertEquals("ABC - TESTPROD - 12-Dec-22", receiveLine.HumanReadableShortcutName);
		}

		#endregion

		#endregion

		#endregion

		#region Property Infos

		// Persistent Properties

		public void TestWE_ClientOrderedUnitsInfo_ReadOnly()
		{
			TestStandardReadOnly(d => d.WE_ClientOrderedUnitsInfo);
		}

		public void TestWE_ClientOrderedUnitsInfo_StartedReceivingReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			TestStartedReceivingReadOnly(receiveLine.WE_ClientOrderedUnitsInfo, true);
		}

		protected override void TestWE_TransactionQuantityInfoCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			TestStartedReceivingReadOnly(receiveLine.WE_TransactionQuantityInfo, false);
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(receiveLine.WE_TransactionQuantityInfo, data);
		}

		void TestStartedReceivingReadOnly(ZPropertyInfo info, bool expectedReadOnlyAfterStartedReceiving)
		{
			var receiveLine = (WhsReceiveLine)info.BizObj;
			var receive = receiveLine.Docket;
			AssertEquals("Precondition", false, receive.StartedReceiving);
			AssertEquals(!expectedReadOnlyAfterStartedReceiving, info.ReadOnly);

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			AssertEquals(expectedReadOnlyAfterStartedReceiving, info.ReadOnly);
		}

		protected override void TestWE_LineNoInfoCore()
		{
			TestStandardReadOnly(d => d.WE_LineNoInfo);
		}

		protected override void TestWE_SubLineNoInfoCore()
		{
			TestStandardReadOnly(d => d.WE_SubLineNoInfo);
		}

		protected override void TestWE_WLInfoCore(TestDataSimpleEnvironment data, ZPropertyInfo info)
		{
			base.TestWE_WLInfoCore(data, info);

			var receiveLine = (WhsReceiveLine)info.BizObj;
			var receive = receiveLine.Docket;
			receive.WD_WW_Whs = Factory.New<WhsWarehouse>().PK;
			AssertEquals("Precondition", false, receiveLine.WE_WLInfo.ReadOnly);

			receive.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("If warehouse is not specified, then location should be readonly.", true, receiveLine.WE_WLInfo.ReadOnly);
		}

		public void TestWE_ReceiveCrossDockOrderNoInfo()
		{
			TestStandardReadOnly(d => d.WE_ReceiveCrossDockOrderNoInfo);
		}

		protected override void TestWE_PalletIDInfoCore(TestDataSimpleEnvironment data)
		{
			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			AssertEquals("Precondition", false, docketLine.WE_PalletIDInfo.ReadOnly);

			docketLine.IsInventoryEditForm = true;
			AssertEquals("Pallet ID should be readonly on Inventory Edit form.", true, docketLine.WE_PalletIDInfo.ReadOnly);
		}

		[TestDate(2018, 7, 26, 12, 35, 55)]
		public void TestSetWE_UnloadedTime_WhenInventoryStatusIsReceived_NotSaved()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals("WE_UnloadedTime", ZDateTimeOffset.Empty, receiveLine.WE_UnloadedTime);

			var unloadDateTime1 = new ZDateTimeOffset(TestDateAttribute.Date);
			receiveLine.WE_WL = dockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("WE_UnloadedTime", unloadDateTime1, receiveLine.WE_UnloadedTime);

			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals("WE_UnloadedTime should be cleared.", ZDateTimeOffset.Empty, receiveLine.WE_UnloadedTime);

			TestDateAttribute.AddHours(1);
			var unloadDateTime2 = new ZDateTimeOffset(TestDateAttribute.Date);
			AssertNotEquals("Precondition: unloadDateTime1 and unloadDateTime2 are not equal.", unloadDateTime2, unloadDateTime1);
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			AssertEquals("WE_UnloadedTime should be updated.", unloadDateTime2, receiveLine.WE_UnloadedTime);
		}

		[TestDate(2018, 7, 26, 12, 35, 55)]
		public void TestSetWE_UnloadedTime_WhenInventoryStatusIsReceived_Saved()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
			AssertEquals("WE_UnloadedTime", ZDateTimeOffset.Empty, receiveLine.WE_UnloadedTime);

			var unloadDateTime1 = new ZDateTimeOffset(TestDateAttribute.Date);
			receiveLine.WE_WL = dockDoorLocation.PK;
			AssertEquals("Precondition", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("WE_UnloadedTime", unloadDateTime1, receiveLine.WE_UnloadedTime);
			Factory.Save();

			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
			AssertEquals("WE_UnloadedTime should not be cleared.", unloadDateTime1, receiveLine.WE_UnloadedTime);

			TestDateAttribute.AddHours(1);
			var unloadDateTime2 = new ZDateTimeOffset(TestDateAttribute.Date);
			AssertNotEquals("Precondition: unloadDateTime1 and unloadDateTime2 are not equal.", unloadDateTime2, unloadDateTime1);
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			AssertEquals("WE_UnloadedTime should not be updated.", unloadDateTime1, receiveLine.WE_UnloadedTime);
		}

		public void TestSetWE_GS_NKUnloadedBy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Precondition", true, receiveLine.WE_UnloadedTime.IsEmpty);
			AssertEquals("Precondition", true, receiveLine.WE_GS_NKUnloadedBy.IsEmpty);

			receiveLine.WE_UnloadedTime = ZDateTimeOffset.Now.AddHours(-1);
			AssertEquals("Should set value.", true, receiveLine.WE_UnloadedTime.IsValid);
			AssertEquals("Should set value.", GlbStaff.CurrentUser.GS_Code, receiveLine.WE_GS_NKUnloadedBy);

			var firstUserCode = GlbStaff.CurrentUser.GS_Code;
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertNotEquals("The user must be different from the previous user.", staff.GS_Code, firstUserCode);

				receiveLine.WE_UnloadedTime = ZDateTimeOffset.Now;
				AssertEquals("Should update value if new user set the time (we can keep old value if needed).", "ABC", receiveLine.WE_GS_NKUnloadedBy);
			}

			receiveLine.WE_UnloadedTime = ZDateTimeOffset.Empty;
			AssertEquals("Should clear value when unload time is empty.", "", receiveLine.WE_GS_NKUnloadedBy);
		}

		#region PartAttributes

		protected override void TestWE_PartAttrib1InfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PartAttrib1InfoCore(data);

			var receiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery());
			var inventory = receiveLine.Inventory.AddNew();
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			var part = receiveLine.SupplierPart;
			Helper.SetProductAttributeUse(receiveLine.Docket.Client, part, AttributeNumber.One, false);
			TestTempProductReadOnly(receiveLine, part, receiveLine.WE_PartAttrib1Info);
			TestPartAttribibNotReaonlyIfItHasValue(receiveLine, part, receiveLine.WE_PartAttrib1Info);
		}

		protected override void TestWE_PartAttrib2InfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PartAttrib2InfoCore(data);

			var receiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery());
			var inventory = receiveLine.Inventory.AddNew();
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			var part = receiveLine.SupplierPart;
			Helper.SetProductAttributeUse(receiveLine.Docket.Client, part, AttributeNumber.Two, false);
			TestTempProductReadOnly(receiveLine, part, receiveLine.WE_PartAttrib2Info);
			TestPartAttribibNotReaonlyIfItHasValue(receiveLine, part, receiveLine.WE_PartAttrib2Info);
		}

		protected override void TestWE_PartAttrib3InfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PartAttrib3InfoCore(data);

			var receiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery());
			var inventory = receiveLine.Inventory.AddNew();
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			var part = receiveLine.SupplierPart;
			Helper.SetProductAttributeUse(receiveLine.Docket.Client, part, AttributeNumber.Three, false);
			TestTempProductReadOnly(receiveLine, part, receiveLine.WE_PartAttrib3Info);
			TestPartAttribibNotReaonlyIfItHasValue(receiveLine, part, receiveLine.WE_PartAttrib3Info);
		}

		protected override void TestWE_SerialNumberInfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_SerialNumberInfoCore(data);

			var receiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery());
			var inventory = receiveLine.Inventory.AddNew();
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			var part = receiveLine.SupplierPart;
			Helper.SetProductAttributeUse(receiveLine.Docket.Client, part, AttributeNumber.Serial, false);

			TestTempProductReadOnly(receiveLine, part, receiveLine.WE_SerialNumberInfo);

			receiveLine.WE_OP = part.PK;
			AssertEquals("Precondition", true, receiveLine.WE_SerialNumberInfo.ReadOnly);

			receiveLine.WE_SerialNumberInfo.Value = new ZString("BLA");
			AssertEquals("Part Attrib should not be readonly if there is value populated.", false, receiveLine.WE_SerialNumberInfo.ReadOnly);
		}

		protected override void TestWE_ExpiryDateInfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_ExpiryDateInfoCore(data);

			var receiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery());
			var inventory = receiveLine.Inventory.AddNew();
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			var part = receiveLine.SupplierPart;
			Helper.SetProductAttributeUse(receiveLine.Docket.Client, part, AttributeNumber.ExpiryDate, false);
			TestTempProductReadOnly(receiveLine, part, receiveLine.WE_ExpiryDateInfo);
			TestPartAttribibNotReaonlyIfItHasValue(receiveLine, part, receiveLine.WE_ExpiryDateInfo);
		}

		protected override void TestWE_PackingDateInfoCore(TestDataSimpleEnvironment data)
		{
			base.TestWE_PackingDateInfoCore(data);

			var receiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery());
			var inventory = receiveLine.Inventory.AddNew();
			inventory.WI_WE_InDocketLine = receiveLine.PK;
			var part = receiveLine.SupplierPart;
			Helper.SetProductAttributeUse(receiveLine.Docket.Client, part, AttributeNumber.PackingDate, false);
			TestTempProductReadOnly(receiveLine, part, receiveLine.WE_PackingDateInfo);
			TestPartAttribibNotReaonlyIfItHasValue(receiveLine, part, receiveLine.WE_PackingDateInfo);
		}

		void TestPartAttribibNotReaonlyIfItHasValue(WhsReceiveLine receiveLine, OrgSupplierPart part, ZPropertyInfo info)
		{
			receiveLine.WE_OP = part.PK;
			AssertEquals("Precondition", true, info.ReadOnly);

			info.Value = (info.Value is ZString)
					? new ZString("BLA")
					: ZDate.Today;
			AssertEquals("Part Attrib should not be readonly if there is value populated.", false, info.ReadOnly);

			Helper.SetClientAllAttributeType(receiveLine.Docket.Client, false);
			AssertEquals("Part Attrib should not be readonly if there is value populated.", false, info.ReadOnly);
		}

		#endregion

		public void TestWE_RequiredByDateInfo()
		{
			TestStandardReadOnly(d => d.WE_RequiredByDateInfo);
		}

		// Calculated Properties

		#region TestConsigneeNameOrPKInfo

		public void TestConsigneeNameOrPKInfo()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals("ConsigneeNameOrPKInfo should wrap Doc Address Property Info.",
				docketLine.ConsigneeDocAddress.OrganisationNameOrPKInfo, ((ZWrappedPropertyInfo)docketLine.ConsigneeNameOrPKInfo).InnerInfo);
			TestStandardReadOnly(docketLine.ConsigneeNameOrPKInfo);
		}

		#endregion

		#region TestOriginalHoldReasonInfo

		public void TestOriginalHoldReasonInfo()
		{
			var docketLine = GetNewBusinessObject(GetNewWhsDocket());
			AssertEquals("HoldReasonToSetAtFinalisationInfo should wrap WE_CurrentHoldReasonInfo.",
				docketLine.WE_CurrentHoldReasonInfo, ((ZWrappedPropertyInfo)docketLine.OriginalHoldReasonInfo).InnerInfo);
			TestHeldCodeReadOnly(docketLine.OriginalHoldReasonInfo);
		}

		#endregion

		#region TestProductUQInfo

		public void TestProductUQInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, allocateLocations: false, finalise: false);
			Factory.Save();
			var receiveLine = receive.Lines[0];

			AssertEquals("ProductUQ", receiveLine.ProductUQInfo.Name);
			TestTempProductReadOnly(receiveLine, data.Part1, receiveLine.ProductUQInfo);
		}

		#endregion

		#region TestSplitQuantityInfo

		public void TestSplitQuantityInfo()
		{
			TestStandardReadOnly(d => d.SplitQuantityInfo);
		}

		#endregion

		// Base methods

		#region TestTempProductReadOnly

		protected void TestTempProductReadOnly(WhsReceiveLine receiveLine, OrgSupplierPart part, ZPropertyInfo info)
		{
			receiveLine.WE_OP = Guid.Empty;
			TestReadOnly(info, true, true, true, true);

			receiveLine.WE_OP = part.PK;
			TestReadOnly(info, true, true, true, true);

			receiveLine.WE_OP = ZGuid.Invalid;
			receiveLine.ProductCode = "NewProduct";
			TestReceiveEditFormReadonly(info);
		}

		#endregion

		#region TestStandardReadOnly

		protected override void TestStandardReadOnly(ZPropertyInfo info)
		{
			base.TestStandardReadOnly(info);

			// Readonly on Inventory form
			var receiveLine = (WhsReceiveLine)info.BizObj;
			var receive = receiveLine.Docket;
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Precondition", false, info.ReadOnly);

			receiveLine.IsInventoryEditForm = true;
			AssertEquals("This property should be readonly on Inventory Form.", true, info.ReadOnly);
			receiveLine.IsInventoryEditForm = false; // clean up

			// Readonly when created by work order
			var workOrder = Factory.New<WhsWorkOrder>();
			receive.WD_WD_ParentDocket = workOrder.PK;
			AssertEquals("This property should be readonly if the receive line was created by WorkOrder.", true, info.ReadOnly);
			receive.WD_WD_ParentDocket = ZGuid.Empty; // clean up
			workOrder.Delete();
		}

		#endregion

		#region TestProductReadOnly

		protected override void TestProductReadOnly(ZPropertyInfo info)
		{
			base.TestProductReadOnly(info);
			TestInventoryStatusReadOnly(info);
		}

		#endregion

		#region TestAfterPickProductAndAttribsReadOnly

		protected override void TestAfterPickProductAndAttribsReadOnly(ZPropertyInfo info)
		{
			TestReceiveEditFormReadonly(info);
		}

		#endregion

		#region TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly

		protected override void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			TestStandardReadOnly(info);
			TestInventoryStatusReadOnly(info, data);
		}

		#endregion

		#region TestReceiveEditFormReadonly

		void TestReceiveEditFormReadonly(ZPropertyInfo info)
		{
			var receiveLine = (WhsReceiveLine)info.BizObj;
			var receive = receiveLine.Docket;
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Precondition", false, info.ReadOnly);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			AssertEquals("The property should be readonly when unload completed time set and WD_HoldPalletIDPutaway unset.", true, info.ReadOnly);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			receive.WD_HoldPalletIDPutaway = false;
			AssertEquals("The property should not be readonly when unload completed time clear.", false, info.ReadOnly);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			receive.WD_HoldPalletIDPutaway = true;
			AssertEquals("The property should not be readonly when unload completed time clear.", false, info.ReadOnly);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = true;
			AssertEquals("The property should not be readonly when WD_HoldPalletIDPutaway set.", false, info.ReadOnly);

			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			receiveLine.WE_FinalisedDate = DateTime.Now;
			AssertEquals("When line is finalised then the property should be readonly.", true, info.ReadOnly);
			receiveLine.WE_DocketLineStatus = ""; // clean up

			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals("When receive is cancelled then the property should be readonly.", true, info.ReadOnly);

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("When receive is finalised then the property should be readonly.", true, info.ReadOnly);
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.Entered; // clean up

			receiveLine.IsInventoryEditForm = true;
			AssertEquals("The property should be readonly on inventory edit form.", true, info.ReadOnly);

			receiveLine.IsInventoryEditForm = false;
			AssertEquals("Check readonly is cleared in the end.", false, info.ReadOnly);
		}

		#endregion

		#region TestInventoryStatusReadOnly

		void TestInventoryStatusReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data = null)
		{
			var receiveLine = (WhsReceiveLine)info.BizObj;
			var receive = receiveLine.Docket;

			AssertEquals("Precondition", false, info.ReadOnly);
			if (data != null)
			{
				receive.WD_OH_Client = data.Org1.PK;
				receive.WD_WW_Whs = data.Whs1.PK;
				receiveLine.WE_OP = data.Part1.PK;
				receiveLine.WE_F3_NKPackType = "BOX";
			}

			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			receiveLine.WE_WL = receive.Warehouse.DefaultOutboundDockDoorLocation.PK;
			Factory.Save();
			AssertEquals("Property is readonly.", true, info.ReadOnly);

			receiveLine.WE_WL = ZGuid.Empty;
			receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;
			Factory.Save();
			AssertEquals("Property is not readonly.", false, info.ReadOnly);
		}

		#endregion

		#region TestDestLocationReadOnly

		protected override void TestDestLocationReadOnly(ZPropertyInfo info)
		{
			base.TestDestLocationReadOnly(info);
			TestReceiveEditFormReadonly(info);

			var receive = ((WhsReceiveLine)info.BizObj).Docket;
			AssertEquals("Precondition", false, info.ReadOnly);

			receive.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("When warehouse is not entered, then the property should be readonly.", true, info.ReadOnly);
		}

		#endregion

		#region TestDestPalletIDReadonly

		protected override void TestDestPalletIDReadonly(ZPropertyInfo info)
		{
			base.TestDestPalletIDReadonly(info);
			TestReceiveEditFormReadonly(info);
		}

		#endregion

		#region TestHeldCodeReadOnly

		protected override void TestHeldCodeReadOnly(ZPropertyInfo info)
		{
			base.TestHeldCodeReadOnly(info);

			var line = (WhsDocketLine)info.BizObj;
			var docket = line.Docket;

			Assert("Precondition", !info.ReadOnly);
			line.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;
			Assert("Held Code should be readonly if docket line status is PFU.", info.ReadOnly);
			line.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;

			Assert("Precondition", !info.ReadOnly);
			line.IsInventoryEditForm = true;
			Assert("Held Code should be readonly if docket line is InventoryEditForm.", info.ReadOnly);
			line.IsInventoryEditForm = false;

			Assert("Precondition", !info.ReadOnly);
			line.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			line.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			Assert("Held Code should be readonly if DocketLineStatus is finalised.", info.ReadOnly);
			line.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			line.WE_FinalisedDate = ZDateTimeOffset.Empty;

			TestHeldCodeReadOnly_IfDocketCancelled(info);
			TestHeldCodeReadOnly_IfDocketFinalised(info);

			Assert("Precondition", !info.ReadOnly);
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.PuttingAway;
			Assert("Held Code should be readonly if OriginalInventoryStatus is PuttingAway.", info.ReadOnly);
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;

			Assert("Precondition", !info.ReadOnly);
			docket.WD_WD_ParentDocket = Factory.New<WhsWorkOrder>().PK;
			Assert("Held Code should be readonly if the receive is created from work order.", info.ReadOnly);
		}

		void TestHeldCodeReadOnly_IfDocketCancelled(ZPropertyInfo info)
		{
			Assert("Precondition", !info.ReadOnly);
			var originalLine = (WhsDocketLine)info.BizObj;
			var docket = (WhsDocket)originalLine.Docket.Clone();
			var line = (WhsDocketLine)originalLine.Clone();
			line.WE_WD = docket.PK;

			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Assert("Held Code should be readonly if parent docket is cancelled.", line.ZPropertyInfoHash[info.Name].ReadOnly);

			//Clean up
			docket.Delete();
		}

		void TestHeldCodeReadOnly_IfDocketFinalised(ZPropertyInfo info)
		{
			Assert("Precondition", !info.ReadOnly);
			var originalLine = (WhsDocketLine)info.BizObj;
			var docket = (WhsDocket)originalLine.Docket.Clone();
			var line = (WhsDocketLine)originalLine.Clone();
			line.WE_WD = docket.PK;

			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			Assert("Held Code should be readonly if parent docket is finalised.", line.ZPropertyInfoHash[info.Name].ReadOnly);

			//Clean up
			docket.Delete();
		}

		#endregion

		#endregion

		#region TestNonPersistantPropertiesAccessingInventoryInfo

		public void TestNonPersistantPropertiesAccessingInventoryInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, allocateLocations: false, finalise: false);
			Factory.Save();
			var receiveLine = receive.Lines[0];

			AssertEquals("SplitQuantity", receiveLine.SplitQuantityInfo.Name);
		}

		#endregion

		#region Flags

		#region TestIsDocketPutaway

		public virtual void TestIsDocketPutaway()
		{
			var receiveLine = GetNewBusinessObject();
			AssertEquals("Precondition", false, receiveLine.IsDocketPuttingAway);

			var receive = GetNewWhsDocket(receiveLine);
			AssertEquals("Precondition", false, receiveLine.IsDocketPuttingAway);

			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals(true, receiveLine.IsDocketPuttingAway);
			AssertEquals(receive.IsPuttingAway, receiveLine.IsDocketPuttingAway);

			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(false, receiveLine.IsDocketPuttingAway);
			AssertEquals(receive.IsPuttingAway, receiveLine.IsDocketPuttingAway);
		}

		#endregion

		#region TestIsTemporaryProduct

		protected override void TestIsTemporaryProductCore()
		{
			var receiveLine = DocketLine;
			AssertEquals(false, receiveLine.IsTemporaryProduct);

			receiveLine.WE_OP = ZGuid.Invalid;
			AssertEquals(false, receiveLine.IsTemporaryProduct);

			receiveLine.ProductCode = "NewProduct";
			AssertEquals(true, receiveLine.IsTemporaryProduct);
		}

		public void TestIsTemporaryProductGuid()
		{
			var receiveLine = DocketLine;
			AssertEquals(true, receiveLine.IsTemporaryProductGuid);

			receiveLine.WE_OP = ZGuid.BrettsGuid;
			AssertEquals(false, receiveLine.IsTemporaryProductGuid);

			receiveLine.WE_OP = ZGuid.Invalid;
			AssertEquals(true, receiveLine.IsTemporaryProductGuid);

			receiveLine.WE_OP = ZGuid.NewZGuid();
			AssertEquals(false, receiveLine.IsTemporaryProductGuid);
		}

		#endregion

		#region TestCanReserveClientOrderedUnits

		public void TestCanReserveClientOrderedUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLine.WE_TransactionQuantity = 20m;

			AssertEquals("Precondition: receive line is not finalised.", false, receiveLine.IsFinalised);
			AssertEquals("Precondition: WE_ClientOrderedUnits > WE_TransactionQuantity", false, receiveLine.WE_ClientOrderedUnits > receiveLine.WE_TransactionQuantity);
			AssertEquals(false, receiveLine.CanReserveClientOrderedUnits);

			receiveLine.WE_TransactionQuantity = 10m;
			AssertEquals("Precondition: WE_ClientOrderedUnits > WE_TransactionQuantity", true, receiveLine.WE_ClientOrderedUnits > receiveLine.WE_TransactionQuantity);
			AssertEquals(true, receiveLine.CanReserveClientOrderedUnits);

			var location = data.Whs1.FindLocation("A-1");
			receiveLine.WE_WL = location.PK;
			receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receiveLine);
			AssertEquals(false, receiveLine.CanReserveClientOrderedUnits);
		}

		#endregion

		#region TestIsCreatedFromPickByBOM

		public void TestIsFromPickByBOM_InDatabase_True()
		{
			TestIsCreatedFromPickByBOM_InDatabase_Core(isInDatabase: true);
		}

		public void TestIsCreatedFromPickByBOM_InDatabase_False()
		{
			TestIsCreatedFromPickByBOM_InDatabase_Core(isInDatabase: false);
		}

		void TestIsCreatedFromPickByBOM_InDatabase_Core(bool isInDatabase)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			var wheelOrderLine = orderLine1.ChildComponentLines.Single();
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var kitPickLine = orderLine1.PickLines.Single();

			if (isInDatabase)
			{
				Factory.Save();
			}

			var createdReceiveLine = kitPickLine.InventoryLine;
			AssertEquals(true, createdReceiveLine.IsCreatedFromPickByBOM);
		}

		public void TestIsCreatedFromPickByBOM_OverPickedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var receiveLine = receive.Lines[0];
			Factory.Save();

			AssertEquals(false, receiveLine.IsCreatedFromPickByBOM);

			var pick = Helper.CreatePickNew();
			receive.WD_WP_ParentPickForReceive = pick.PK;

			AssertEquals(false, receiveLine.IsCreatedFromPickByBOM);
		}

		#endregion

		#endregion

		#region TestPutawayLocationAreaType

		public void TestPutawayLocationAreaType()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var pickingArea = Helper.CreateArea(whs, "PIK", "AT1");
			var putawayArea = Helper.CreateArea(whs, "PUT", "AT2");
			var loc = whs.DefaultLocation;
			loc.WLV_WA_PickingArea = pickingArea.PK;
			loc.WLV_WA_PutawayArea = putawayArea.PK;
			Factory.Save();

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("AT2", docketLine.PutawayLocationAreaType);

			loc.WLV_WA_PutawayArea = ZGuid.Empty;
			AssertEquals("", docketLine.PutawayLocationAreaType);
		}

		#endregion

		#region TestPutawayLocationAreaName

		public void TestPutawayLocationAreaName()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var pickingArea = Helper.CreateArea(whs, "PIK");
			var putawayArea = Helper.CreateArea(whs, "PUT");
			var loc = whs.DefaultLocation;
			loc.WLV_WA_PickingArea = pickingArea.PK;
			loc.WLV_WA_PutawayArea = putawayArea.PK;

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("PUT", docketLine.PutawayLocationAreaName);

			loc.WLV_WA_PutawayArea = ZGuid.Empty;
			AssertEquals("", docketLine.PutawayLocationAreaName);
		}

		public void TestPutawayLocationAreaName_Translatable()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var loc = whs.DefaultLocation;
			var area = loc.PutawayArea;
			area.WA_Name = "Test Area";

			var docketLine = GetNewBusinessObject();
			docketLine.WE_WL = loc.PK;
			AssertEquals("LocationAreaName in English.", "Test Area", docketLine.PutawayLocationAreaName);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "Test Area").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试区"));
				AssertEquals("LocationAreaName in Chinese.", "测试区", docketLine.PutawayLocationAreaName);
			}
		}

		#endregion

		#region TestWE_PackingDateAndWE_ExpiryDate_WithJulianBatchNumber

		public void TestWE_PackingDateAndWE_ExpiryDate_WithJulianBatchNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			AssertWE_PackingDateAndWE_ExpiryDate_WithJulianBatchNumber(data, AttributeNumber.One, WhsDocketLineSchema.WE_PartAttrib1);
			AssertWE_PackingDateAndWE_ExpiryDate_WithJulianBatchNumber(data, AttributeNumber.Two, WhsDocketLineSchema.WE_PartAttrib2);
			AssertWE_PackingDateAndWE_ExpiryDate_WithJulianBatchNumber(data, AttributeNumber.Three, WhsDocketLineSchema.WE_PartAttrib3);
		}

		void AssertWE_PackingDateAndWE_ExpiryDate_WithJulianBatchNumber(TestDataSimpleEnvironment data, AttributeNumber attributeNumber, SchemaColumn partAttributeColumn)
		{
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 9001;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1-1"), "a");
			AssertEquals("Precondition: Expiry date should be read only.", true, receiveLine.WE_ExpiryDateInfo.ReadOnly);
			AssertEquals("Precondition: Packing date should be read only.", true, receiveLine.WE_PackingDateInfo.ReadOnly);

			receiveLine[partAttributeColumn] = "7093 A467 789F";

			AssertEquals($"Expiry date should be read only when using Julian Batch Number in part attribute {attributeNumber}.", true, receiveLine.WE_ExpiryDateInfo.ReadOnly);
			AssertEquals($"Packing date should be read only when using Julian Batch Number in part attribute {attributeNumber}.", true, receiveLine.WE_PackingDateInfo.ReadOnly);
		}

		#endregion

		#region TestWE_WL_DockDoorLocation

		public void TestWE_WL_DockDoorLocation()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var finalisedInventoryLine = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, nonDockDoorLocation, "", false, true).Lines.Single();
			var receiveWithArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", now);
			var receiveWithNoArrivalDate = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R3", ZDateTimeOffset.Empty);
			var lineWithArrivalDate = receiveWithArrivalDate.Lines.AddNew();
			var lineWithoutArrivalDate = receiveWithNoArrivalDate.Lines.AddNew();
			AssertEquals("Precondition", now, lineWithArrivalDate.WE_AdjustmentArrivalDate);
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, lineWithArrivalDate.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, lineWithoutArrivalDate.WE_OriginalInventoryStatus);

			lineWithArrivalDate.WE_WL = nonDockDoorLocation.PK;
			lineWithoutArrivalDate.WE_WL = nonDockDoorLocation.PK;
			AssertEquals(InventoryStatus.Codes.Putaway, lineWithArrivalDate.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, lineWithoutArrivalDate.WE_OriginalInventoryStatus);

			lineWithArrivalDate.WE_WL = ZGuid.Empty;
			lineWithoutArrivalDate.WE_WL = ZGuid.Empty;
			AssertEquals(InventoryStatus.Codes.Arrived, lineWithArrivalDate.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Pending, lineWithoutArrivalDate.WE_OriginalInventoryStatus);

			lineWithArrivalDate.WE_WL = nonDockDoorLocation.PK;
			lineWithoutArrivalDate.WE_WL = nonDockDoorLocation.PK;
			AssertEquals(InventoryStatus.Codes.Putaway, lineWithArrivalDate.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, lineWithoutArrivalDate.WE_OriginalInventoryStatus);
		}

		#endregion

		#region TestWE_PalletID_ValueChanged

		public void TestWE_PalletID_ValueChanged()
		{
			var refreshBindingCalled = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");

			receive.TotalPalletsReceivedInfo.ValueChanged += (s, e) => refreshBindingCalled = true;

			AssertEquals("Precondition:", 2, receive.TotalPalletsReceived);
			AssertEquals("Precondition:", false, refreshBindingCalled);

			receiveLine3.WI_PalletID = "PLT3";
			AssertEquals("Change PalletID to unique PalletID, should still have 2 PalletIDs", 2, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			receiveLine3.WI_PalletID = "PLT1";
			AssertEquals("Change PalletID to non-unique PalletID, should have 1 PalletID", 1, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);
		}

		#endregion

		#region TestCancelAndReactivateReceiveLine

		public void TestCancelAndReactivateReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();
			AssertEquals("Precondition", 10m, receiveLine.WE_StockOnHand);

			var inventory = receiveLine.Inventory[0];
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals("Inventory should have 2 reserved PickLines.", 2, inventory.ReservedPickLines.Count);

			receiveLine.CancelLine();
			AssertEquals("TotalUnits in canceled line should be zero.", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Cancelling the Receive Line should have deleted the reserved PickLines.", 0, inventory.ReservedPickLines.Count);
			AssertEquals("Reserved PickLine 1 should be deleted.", true, reservedPickLine1.IsDeleted);
			AssertEquals("Reserved PickLine 2 should be deleted.", true, reservedPickLine2.IsDeleted);

			receiveLine.ReactivateLine();
			AssertEquals("Total units in ReactivateLine should back to original value.", 10m, receiveLine.WE_StockOnHand);
		}

		#endregion

		#region TestIsPutaway

		public void TestIsPutawayReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var location = data.Whs1.FindLocation("A-1");
			receiveLine.WE_WL = location.PK;
			Factory.Save();

			AssertEquals("Receive line is Putaway.", true, receiveLine.IsPutaway);
		}

		public void TestIsPutawayReceiveLine_DockdoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation);
			Factory.Save();

			AssertEquals("Receive line is not Putaway.", false, receiveLine.IsPutaway);
		}

		public void TestIsPutawayReceiveLine_CrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WL_CrossDock = dockDoorLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventoryLine);
			AssertEquals("Precondition: Stock is cross-docked.", 10m, reservedPickLine.ReservedQuantity);
			AssertEquals("Precondition: location is a dockdoor location", true, dockDoorLocation.IsDockDoorLocation);
			Factory.Save();

			var receiveLine = (WhsReceiveLine)inventoryLine.InDocketLine;
			receiveLine.WE_WL = dockDoorLocation.PK;
			AssertEquals("Receive line is Putaway.", true, receiveLine.IsPutaway);
		}

		public void TestIsCrossDockLocation_CrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WL_CrossDock = dockDoorLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventoryLine);
			AssertEquals("Precondition: Stock is cross-docked.", 10m, reservedPickLine.ReservedQuantity);
			AssertEquals("Precondition: location is a dockdoor location", true, dockDoorLocation.IsDockDoorLocation);
			Factory.Save();

			var receiveLine = (WhsReceiveLine)inventoryLine.InDocketLine;
			AssertEquals("Location is the crossdock location.", true, receiveLine.IsCrossDockLocation(dockDoorLocation));
		}

		public void TestIsCrossDockLocation_NotCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var location = data.Whs1.DefaultOutboundDockDoorLocation;
			Factory.Save();

			AssertEquals("Location is not a crossdock location.", false, receiveLine.IsCrossDockLocation(location));
		}

		#endregion

		#region TestInventoryStatusReceivedReadOnly

		public void TestInventoryStatusReceivedReadOnly_ReceiveLineInDatabase_InventoryStatusReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			SetAllAttributeUseForProduct(data.Org1, data.Part1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertPropertyInfosReadOnly(receiveLine, false, "Precondition: Property info is not read only.");

			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			receiveLine.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			AssertEquals("Precondition: Original inventory status is received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Original inventory status has changes.", true, receiveLine.WE_OriginalInventoryStatusInfo.HasChanges);
			AssertPropertyInfosReadOnly(receiveLine, false, "Property info is not read only.");

			Factory.Save();

			AssertEquals("Original inventory status has no changes.", false, receiveLine.WE_OriginalInventoryStatusInfo.HasChanges);
			AssertPropertyInfosReadOnly(receiveLine, true, "Property info's read only status is correct.");
			AssertNoExceptionThrown("Delete is still possible.", receiveLine.Delete);
			AssertNoExceptionThrown("Delete is still possible.", Factory.Save);
		}

		public void TestInventoryStatusReceivedReadOnly_ReceiveLineInDatabase_InventoryStatusNotReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			SetAllAttributeUseForProduct(data.Org1, data.Part1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertPropertyInfosReadOnly(receiveLine, false, "Precondition: Property info is not read only.");
			receiveLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Pending;

			AssertNotEquals("Precondition: Original inventory status is not received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Original inventory status has changes.", true, receiveLine.WE_OriginalInventoryStatusInfo.HasChanges);
			AssertPropertyInfosReadOnly(receiveLine, false, "Property info is not read only.");

			Factory.Save();

			AssertEquals("Original inventory status has no changes.", false, receiveLine.WE_OriginalInventoryStatusInfo.HasChanges);
			AssertPropertyInfosReadOnly(receiveLine, false, "Property info's read only status is correct.");
		}

		public void TestInventoryStatusReceivedReadOnly_ReceiveLineNotInDatabase_InventoryStatusReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			SetAllAttributeUseForProduct(data.Org1, data.Part1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			receiveLine.WE_WL_TransferFrom = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			AssertEquals("Precondition: original inventory status is received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: original inventory status has no changes.", false, receiveLine.WE_OriginalInventoryStatusInfo.HasChanges);
			AssertPropertyInfosReadOnly(receiveLine, false, "Precondition: Property info is not read only.");
		}

		void AssertPropertyInfosReadOnly(WhsReceiveLine receiveLine, bool expectedReadOnly, string assertionMessage)
		{
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_WLInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_OPInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_PartAttrib1Info.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_PartAttrib2Info.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_PartAttrib3Info.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_SerialNumberInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_PackingDateInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_ExpiryDateInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_PalletIDInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_TransactionQuantityInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_PackQuantityInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(assertionMessage, expectedReadOnly, receiveLine.WE_WHC_NKOriginalInventoryHeldCodeInfo.ReadOnly);
		}

		void SetAllAttributeUseForProduct(OrgHeader owner, OrgSupplierPart product)
		{
			Helper.SetClientAttributeType(owner, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(owner, AttributeNumber.Two, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetClientAttributeType(owner, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(owner, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(owner, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(owner, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(owner, product, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(owner, product, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(owner, product, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(owner, product, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(owner, product, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(owner, product, AttributeNumber.PackingDate, true);
		}

		#endregion

		#region TestPackingDateCalculatesExpiryDate

		public void TestPackingDateCalculatesExpiryDate_TemporaryProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = receive.Lines.AddNew();
			receiveLine.ProductCode = "NEWPRODUCT";
			receiveLine.WE_TransactionQuantity = 10m;

			AssertEquals("Precondition: receive line has a temporary product.", true, receiveLine.IsTemporaryProduct);
			AssertEquals("Precondition: Packing date is empty.", ZDate.Empty, receiveLine.WE_PackingDate);
			AssertEquals("Precondition: Packing date is not readonly.", false, receiveLine.WE_PackingDateInfo.ReadOnly);
			AssertEquals("Precondition: Expiry date is empty.", ZDate.Empty, receiveLine.WE_ExpiryDate);
			AssertEquals("Precondition: Expiry date is not readonly.", false, receiveLine.WE_ExpiryDateInfo.ReadOnly);

			receiveLine.WE_PackingDate = new ZDate(2020, 04, 15);
			AssertEquals("Expiry date is not calculated from packing date and shelf life.", ZDate.Empty, receiveLine.WE_ExpiryDate);
		}

		protected override bool TestDocketLineCanCalculateExpiryDateFromPackingDate => true;

		#endregion

		#region TestOperationalActionVisibility_Receive

		public void TestOperationalActionVisibility_Receive()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsReceiveLine).GetProperty(nameof(WhsReceiveLine.OriginalHoldReason))).ReadOnly);
		}

		#endregion

		#region TestCanHavePutawayTransfer

		public void TestCanHavePutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var receiveLine = GetNewBusinessObject();
			Assert("Can have putaway transfer only if is picked for upload or has valid location and original inventory status should be Received.", !receiveLine.CanHavePutawayTransfer);

			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;
			Assert("Can have putaway transfer when is picked for upload or has valid location and original inventory status should be Received.", receiveLine.CanHavePutawayTransfer);

			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.HeldForTransfer; // any status except PickedForUnload
			Assert("Can have putaway transfer only if is picked for upload or has valid location and original inventory status should be Received.", !receiveLine.CanHavePutawayTransfer);

			receiveLine.WE_WL = data.Whs1.DefaultLocation.PK;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			Assert("Can have putaway transfer when is picked for upload or has valid location and original inventory status should be Received.", receiveLine.CanHavePutawayTransfer);

			receiveLine.WE_WL = ZGuid.Invalid;
			Assert("Can have putaway transfer only if is picked for upload or has valid location and original inventory status should be Received.", !receiveLine.CanHavePutawayTransfer);

			receiveLine.WE_WL = data.Whs1.DefaultLocation.PK;
			receiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway; // any status except Received
			Assert("Can have putaway transfer only if is picked for upload or has valid location and original inventory status should be Received.", !receiveLine.CanHavePutawayTransfer);
		}

		#endregion

		#region TestSerialNumbers

		public void TestSerialNumbers()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
				var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 3m);

				for (int i = 1; i <= 3; i++)
				{
					var sn = Helper.CreateWhsSerialNumber(data.Org1, data.Part1, $"SN{i}");
					CreateWhsSerialNumberPivot(receiveLine1.PK, sn);
				}

				AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2", "SN3" }, receiveLine1.SerialNumbers.Select(c => c.SerialNumberValue));
				AssertEquals(0, receiveLine2.SerialNumbers.Count);

				void CreateWhsSerialNumberPivot(ZGuid parentPK, WhsSerialNumber serialNumber)
				{
					var serialNumberPivot = Factory.New<WhsSerialNumberPivot>();
					serialNumberPivot.WSV_ParentID = parentPK;
					serialNumberPivot.WSV_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
					serialNumberPivot.WSV_IsReleaseCaptured = false;
					serialNumberPivot.WSV_WSN_SerialNumber = serialNumber.PK;
				}
			}
		}

		#endregion

		#region TestISerialNumberParentMembers

		public void TestISerialNumberParentMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1, true, false);
			ISerialNumberParent receiveLine = receive.Lines[0];

			AssertEquals(receiveLine.PK, receiveLine.PK);
			AssertEquals(data.Org1.PK, receiveLine.ClientPK);
			AssertEquals(data.Part1.PK, receiveLine.ProductPK);
			AssertEquals(WhsDocketLineSchema.Constants.Prefix, receiveLine.TablePrefix);
			AssertEquals(true, receiveLine.SerialNumberReadOnly);
			AssertEquals(false, receiveLine.IsInDatabase);
			AssertEquals(true, receiveLine.IsAllowedToCreateOriginalSerialNumberRecord);
			AssertEquals(receive.Factory, receiveLine.Factory);
			AssertEquals(typeof(WhsSerialNumberPivotCollection), receiveLine.SerialNumbers.GetType());

			Factory.Save();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var pivot = receiveLine.SerialNumbers.AddNew();
			pivot.SerialNumberValue = "SN1";
			AssertEquals(false, receiveLine.IsSerialNumberAlreadyInUse(pivot));
			AssertEquals(true, receiveLine.IsInDatabase);
			AssertEquals(false, receiveLine.SerialNumberReadOnly);

			receive.WD_OH_Client = ZGuid.Empty;
			receive.Lines[0].WE_OP = ZGuid.Empty;
			AssertEquals("Should return value without exception.", true, receiveLine.SerialNumberReadOnly);
		}

		#endregion

		#region TestIsSerialNumberUsed

		public void TestIsSerialNumberUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m);
			Factory.Save();

			AssertEquals(false, receiveLine.IsSerialNumberUsed);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			AssertEquals(true, receiveLine.IsSerialNumberUsed);

			receive.WD_OH_Client = ZGuid.Empty;
			receiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("Should return value without exception.", false, receiveLine.IsSerialNumberUsed);
		}

		#endregion

		#region TestReceiveLine_SerialNumber_EditSerialNumber_AsnLine

		public void TestReceiveLine_SerialNumber_EditSerialNumber_NoAsnLine()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = "SN1";
				Factory.Save();
				AssertEquals("Precondition", "SN1", receiveLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Precondition", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Precondition", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));

				pivot.SerialNumberValue = "SN2";
				AssertEquals("Should update serial number.", "SN2", receiveLine.SerialNumbers.Single().SerialNumberValue);
				Factory.Save();
				AssertEquals("Should not create new serial number.", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Should not create extra serial number pivot.", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
				AssertEquals(true, receiveLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
			}
		}

		public void TestReceiveLine_SerialNumber_EditSerialNumber_HasAsnLine_NotSerialNumberUsed()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = "SN1";
				var serialNumberPK = pivot.WSV_WSN_SerialNumber;
				Factory.Save();
				receive.PopulateASNLines();
				Factory.Save();
				AssertEquals("Precondition", "SN1", receiveLine.SerialNumbers.Single().SerialNumberValue);
				var asnLine = receive.AsnLines.Cast<WhsAsnLine>().Single();
				AssertEquals("Precondition", "SN1", asnLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Precondition", serialNumberPK, receiveLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Precondition", serialNumberPK, asnLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Precondition", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Precondition", 2, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
				AssertEquals("Precondition", true, receiveLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals("Precondition", true, asnLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals(receiveLine.SerialNumbers[0].SerialNumber.PK, asnLine.SerialNumbers[0].SerialNumber.PK);

				pivot.SerialNumberValue = "SN2";
				Factory.Save();
				AssertEquals("Should update serial number.", "SN2", receiveLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Should keep existing ASN line.", "SN1", asnLine.SerialNumbers.Single().SerialNumberValue);
				AssertNotEquals("Should create new serial number.", serialNumberPK, receiveLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Should keep exisiting serial number.", serialNumberPK, asnLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals(true, receiveLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals(false, asnLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertNotEquals(receiveLine.SerialNumbers[0].SerialNumber.PK, asnLine.SerialNumbers[0].SerialNumber.PK);
				AssertEquals("Should create new serial number.", 2, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Should not create extra serial number pivot.", 2, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
			}
		}

		public void TestReceiveLine_SerialNumber_EditSerialNumber_HasAsnLine_NotSerialNumberUsed_EnterSameValue()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = "SN1";
				var serialNumberPK = pivot.WSV_WSN_SerialNumber;
				Factory.Save();
				receive.PopulateASNLines();
				Factory.Save();
				AssertEquals("Precondition", "SN1", receiveLine.SerialNumbers.Single().SerialNumberValue);
				var asnLine = receive.AsnLines.Cast<WhsAsnLine>().Single();
				AssertEquals("Precondition", "SN1", asnLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Precondition", serialNumberPK, receiveLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Precondition", serialNumberPK, asnLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Precondition", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Precondition", 2, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
				AssertEquals("Precondition", true, receiveLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals("Precondition", true, asnLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals(receiveLine.SerialNumbers[0].SerialNumber.PK, asnLine.SerialNumbers[0].SerialNumber.PK);

				pivot.SerialNumberValue = "SN1";
				Factory.Save();
				AssertEquals("Should update serial number.", "SN1", receiveLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Should keep existing ASN line.", "SN1", asnLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Should create new serial number.", serialNumberPK, receiveLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Should keep exisiting serialnumber.", serialNumberPK, asnLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals(true, receiveLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals(true, asnLine.SerialNumbers[0].SerialNumber.WSN_IsInUse);
				AssertEquals(receiveLine.SerialNumbers[0].SerialNumber.PK, asnLine.SerialNumbers[0].SerialNumber.PK);
				AssertEquals("Should not create new serial number.", 1, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Should not create extra serial number pivot.", 2, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
			}
		}

		public void TestReceiveLine_SerialNumber_EditSerialNumber_HasAsnLine_SerialNumberAlreadyUsed()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = "SN1";
				var serialNumberPK = pivot.WSV_WSN_SerialNumber;
				Factory.Save();
				receive.PopulateASNLines();
				AssertEquals("Precondition", "SN1", receiveLine.SerialNumbers.Single().SerialNumberValue);
				var asnLine = receive.AsnLines.Cast<WhsAsnLine>().Single();
				AssertEquals("Precondition", "SN1", asnLine.SerialNumbers.Single().SerialNumberValue);
				AssertEquals("Precondition", serialNumberPK, receiveLine.SerialNumbers.Single().WSV_WSN_SerialNumber);
				AssertEquals("Precondition", serialNumberPK, asnLine.SerialNumbers.Single().WSV_WSN_SerialNumber);

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals(true, receive.IsFinalised);
				Factory.Save();
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				Factory.Save();

				AssertEquals("Should be readonly when use in order.", true, ((ISerialNumberParent)receiveLine).SerialNumberReadOnly);
				AssertExceptionThrown<InvalidOperationException>("Cannot set SerialNumber because it is readonly.", () => pivot.SerialNumberValue = "SN2");
			}
		}

		public void TestReceiveLine_SerialNumber_EditSerialNumber_SwapSerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN2";
				Factory.Save();
				receive.PopulateASNLines();
				AssertContainsExactElementsInAnyOrder(["SN1", "SN2"], receiveLine.SerialNumbers.Select(c => c.SerialNumberValue));
				var asnLine = receive.AsnLines.Cast<WhsAsnLine>().Single();
				AssertContainsExactElementsInAnyOrder(["SN1", "SN2"], asnLine.SerialNumbers.Select(c => c.SerialNumberValue));
				Factory.Save();

				var serialNumberPKs = asnLine.SerialNumbers.Select(s => s.WSV_WSN_SerialNumber).ToArray();
				AssertEquals("Precondition", 2, Factory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Precondition", 4, Factory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));

				pivot1.SerialNumberValue = "SN2";
				AssertHasError(pivot1.SerialNumberValueInfo, "Serial # already used.");
				pivot2.SerialNumberValue = "SN1";
				AssertEquals("Should not have error.", false, pivot2.SerialNumberValueInfo.HasErrors());

				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
				AssertContainsExactElementsInAnyOrder(serialNumberPKs, receiveInNewFactory.AsnLines[0].SerialNumbers.Select(s => s.WSV_WSN_SerialNumber));
				AssertContainsExactElementsInAnyOrder(["SN1", "SN2"], receiveInNewFactory.Lines[0].SerialNumbers.Select(c => c.SerialNumberValue));
				AssertContainsExactElementsInAnyOrder(["SN1", "SN2"], receiveInNewFactory.AsnLines[0].SerialNumbers.Select(c => c.SerialNumberValue));
				AssertEquals("When edit create new one.", 4, newFactory.GetDatabaseCount(typeof(WhsSerialNumber)));
				AssertEquals("Should not create extra serial number pivot.", 4, newFactory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
				AssertEquals("Should create new serial number.", false, serialNumberPKs.Any(sn => receiveInNewFactory.Lines[0].SerialNumbers.Any(r => r.WSV_WSN_SerialNumber.Equals(sn))));
				receiveInNewFactory.RunPreSaveValidation();
				AssertEquals(false, receiveInNewFactory.HasErrors);
			}
		}

		public void TestReceiveLine_SerialNumber_EditSerialNumber_EditAgain()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
				var pivot = receiveLine.SerialNumbers.AddNew();
				pivot.SerialNumberValue = "SN1";
				Factory.Save();
				receive.PopulateASNLines();
				AssertEquals("SN1", receiveLine.SerialNumbers.Single().SerialNumberValue);
				Factory.Save();
				var asnPivotPK = receive.AsnLines.Cast<WhsAsnLine>().Single().SerialNumbers.Single().PK;
				var asnSerialNumberPK = receive.AsnLines.Cast<WhsAsnLine>().Single().SerialNumbers.Single().WSV_WSN_SerialNumber;

				var newSN1 = "SN2";
				receiveLine.SerialNumbers.Single().SerialNumberValue = newSN1;
				AssertEquals(newSN1, receiveLine.SerialNumbers.Single().SerialNumberValue);
				Factory.Save();
				AssertExpectedInDB(newSN1);

				var newSN2 = "SN3";
				receiveLine.SerialNumbers.Single().SerialNumberValue = newSN2;
				AssertEquals(newSN2, receiveLine.SerialNumbers.Single().SerialNumberValue);
				Factory.Save();
				AssertExpectedInDB(newSN2);

				void AssertExpectedInDB(string newSN)
				{
					var newFactory = new BusinessObjectFactory();
					var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
					AssertEquals(newSN, receiveLine.SerialNumbers.Single().SerialNumberValue);
					var asnLine = receiveInNewFactory.AsnLines.Cast<WhsAsnLine>().Single();
					var asnLinePivot = asnLine.SerialNumbers.Single();
					AssertEquals("Should not change.", "SN1", asnLine.SerialNumbers.Single().SerialNumberValue);
					AssertEquals("Should not change.", asnPivotPK, asnLinePivot.PK);
					AssertEquals("Should not change.", asnSerialNumberPK, asnLinePivot.WSV_WSN_SerialNumber);
					AssertEquals("When edit create new one but when edit again it should not create again.", 2, newFactory.GetDatabaseCount(typeof(WhsSerialNumber)));
					AssertEquals("Should not create extra serial number pivot.", 2, newFactory.GetDatabaseCount(typeof(WhsSerialNumberPivot)));
				}
			}
		}

		#endregion

		#endregion

		#region TestConstraints

		#region TestConstraint_WE_CurrentInventoryStatus

		[ExpectNoExceptions]
		public void TestConstraint_WE_CurrentInventoryStatus_FinalizedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var expectedExceptionMsg = "The UPDATE statement conflicted with the CHECK constraint \"Constraint_WE_CurrentInventoryStatus";
			receive.FinaliseDocket();

			// AVL
			Factory.Save();
			AssertEquals("Expected 'AVL' current inventory status.", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Docket line must be finalized.", true, inventoryLine.IsFinalised);

			// HEL
			inventoryLine.WE_WHC_NKCurrentInventoryHeldCode = "COOL";
			inventoryLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			Factory.Save();
			AssertEquals("Expected 'HEL' current inventory status.", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);

			// PUT: Not Allowed
			inventoryLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedExceptionMsg, true), "Exception expected from Constraint_WE_CurrentInventoryStatus");
		}

		#endregion

		#endregion

		#region TestDelete_DeletesConsigneeDocAddress

		public void TestDelete_DeletesConsigneeDocAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var docket = GetNewWhsDocket(data.Org1, data.Whs1);
			var docketLine = GetNewBusinessObject(docket);
			docketLine.ConsigneeDocAddress.OrganisationPK = data.Org1.PK;

			docket.Lines.Delete(docketLine);
			Factory.Save();
			var consigneeDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, docketLine.PK));
			AssertEquals("ConsigneeDocAddress should have been deleted on DocketLine.Delete().", 0, consigneeDocAddress.Length);
		}

		#endregion

		#region TestOnDeleteByInventory

		protected override bool ShouldDocketLineBeDeletedByInventory
		{
			get { return true; }
		}

		#endregion

		#region TestReceiveLineDeletion

		[TestDate(2018, 8, 7, 7, 30, 0)]
		public void TestReceiveLineDeletion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inv2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 1m);
			var inv3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "C", 1m);
			var inv4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inv5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, nonDockDoorLocation);
			var inv6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var lineInPendingStatus = (WhsReceiveLine)inv1.InDocketLine;
			var lineInReceivedStatus = (WhsReceiveLine)inv2.InDocketLine;
			var lineWithPutawayTransfer = (WhsReceiveLine)inv3.InDocketLine;
			var lineInArrivedStatus = (WhsReceiveLine)inv4.InDocketLine;
			lineInArrivedStatus.WE_OriginalInventoryStatus = InventoryStatus.Codes.Arrived;
			lineInArrivedStatus.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			var lineInPutawayStatusDirectPutaway = (WhsReceiveLine)inv5.InDocketLine;
			lineInPutawayStatusDirectPutaway.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 1m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var lineInPickedForUnloadStatus = (WhsReceiveLine)inv6.InDocketLine;
			lineInPickedForUnloadStatus.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			lineInPickedForUnloadStatus.WE_StockOnHand = 0m;
			lineInPickedForUnloadStatus.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;

			transferLineForPalletC.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineForPalletC);

			AssertEquals("Precondition", InventoryStatus.Codes.Pending, lineInPendingStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineInReceivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, lineInArrivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLineForPalletC.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, lineInPutawayStatusDirectPutaway.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", DocketLineStatus.Codes.PickedForUnload, lineInPickedForUnloadStatus.WE_DocketLineStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineWithPutawayTransfer.WE_OriginalInventoryStatus);

			AssertEquals(true, lineInPendingStatus.CanDelete);
			AssertEquals(true, lineInReceivedStatus.CanDelete);
			AssertEquals(false, lineWithPutawayTransfer.CanDelete);
			AssertEquals(true, lineInArrivedStatus.CanDelete);
			AssertEquals(true, lineInPutawayStatusDirectPutaway.CanDelete);
			AssertEquals(false, lineInPickedForUnloadStatus.CanDelete);

			AssertEquals("Putaway receive lines with a putaway transfer must not be deleted.", lineWithPutawayTransfer.ReasonForNotAbleToDelete);
		}

		public void TestReceiveLineDeletion_CanDelete_PutawayTransfer()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "A");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, normalLocation, "A", 10m);
			putawayTransferLine.RunPreSaveValidation();
			AssertNotNull("Precondition: receiveLine exist", receiveLine);
			AssertNotNull("Precondition: putawayTransferLine exists", putawayTransferLine);
			AssertEquals("Precondition: putawayTransferLine exists", 1, putawayTransferLine.PickLines.Count);
			AssertEquals("Can*NOT*Delete a committed Received ReceiveLine.", false, receiveLine.CanDelete);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			var pickline = putawayTransferLine.PickLines.Single();
			AssertEquals("Pickline is picked.", true, pickline.IsPicked);
			AssertEquals("Can*NOT*Delete a picked Received ReceiveLine.", false, receiveLine.CanDelete);
		}

		public void TestReceiveLineDeletion_CanDelete_CreatedFromWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save(); // In memory Receive Lines are always allowed to be deleted.
			AssertEquals("Precondition: receiveLine can be deleted normally.", true, receiveLine.CanDelete);

			var workOrder = Factory.New<WhsWorkOrder>();
			receive.WD_WD_ParentDocket = workOrder.PK;
			AssertEquals("Cannot Delete a ReceiveLine on a Receive created from a Work Order.", false, receiveLine.CanDelete);
			AssertEquals("Reason message is correct.", "Assembled Inventory cannot be deleted.", receiveLine.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDeletedReceiveLineIsNotLoadedForFinalize

		public void TestDeletedReceiveLineIsNotLoadedForFinalize()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client = data.Org1;
			var product = data.Part1;
			client.MiscServ.OM_IMUseSerialNumber = true;
			client.PartAttributeManager.SetProductToUseAttribute(product, 6, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receiveLine.WI_SerialNumber = "PA1";
			Factory.Save();
			AssertEquals("Precondition: Line is in db.", 1, TestConnection.ExecuteScalar("Select Count(*) from dbo.WhsDocketLine;"));

			receiveLine.Delete();

			var receivetLine_2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receivetLine_2.WI_SerialNumber = "PA1";

			AssertNoExceptionThrown(() => receive.FinaliseDocket());
		}

		#endregion

		#region TestReceiveLine_Readonly_DockDoorLocations

		[TestDate(2018, 8, 7, 7, 30, 0)]
		public void TestReceiveLine_Readonly_DockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inv2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 1m);
			var inv4 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "C", 1m);
			var inv5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inv6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, nonDockDoorLocation);
			var lineInPendingStatus = (WhsReceiveLine)inv1.InDocketLine;
			var lineInReceivedStatus = (WhsReceiveLine)inv2.InDocketLine;
			var lineInReceivedStatusWithPutawayTransfer = (WhsReceiveLine)inv4.InDocketLine;
			var lineInArrivedStatus = (WhsReceiveLine)inv5.InDocketLine;
			var lineInPutawayStatusWithoutDockDoorLocation = (WhsReceiveLine)inv6.InDocketLine;
			lineInArrivedStatus.WE_OriginalInventoryStatus = InventoryStatus.Codes.Arrived;
			lineInArrivedStatus.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			lineInPutawayStatusWithoutDockDoorLocation.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 1m);
			transfer.RunPreSaveValidation();
			transferLineForPalletC.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			transferLineForPalletC.WE_WL = nonDockDoorLocation.PK;
			transferLineForPalletC.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineForPalletC);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, lineInPendingStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineInReceivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineInReceivedStatusWithPutawayTransfer.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLineForPalletC.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, lineInArrivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, lineInPutawayStatusWithoutDockDoorLocation.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Inventory Total Units are reduced.", 0m, lineInReceivedStatusWithPutawayTransfer.WE_StockOnHand);
			AssertReadOnlyProperties(lineInPendingStatus, false, false, false, false);
			AssertReadOnlyProperties(lineInReceivedStatus, false, true, true, true);
			AssertReadOnlyProperties(lineInReceivedStatusWithPutawayTransfer, true, true, true, true);
			AssertReadOnlyProperties(lineInArrivedStatus, false, false, false, false);
			AssertReadOnlyProperties(lineInPutawayStatusWithoutDockDoorLocation, false, false, false, false);
		}

		void AssertReadOnlyProperties(WhsReceiveLine docketLine, bool propertiesOtherThanDockDoorReadonly, bool heldCodeReadonly, bool locationReadOnly, bool receivedInventoryStatusReadOnly)
		{
			AssertEquals(receivedInventoryStatusReadOnly, docketLine.WE_OPInfo.ReadOnly);
			AssertEquals(receivedInventoryStatusReadOnly, docketLine.WE_PackQuantityInfo.ReadOnly);
			AssertEquals(receivedInventoryStatusReadOnly, docketLine.WE_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(receivedInventoryStatusReadOnly, docketLine.WE_TransactionQuantityInfo.ReadOnly);
			AssertEquals(locationReadOnly, docketLine.WE_WLInfo.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_ReceiveCrossDockOrderNoInfo.ReadOnly);
			AssertEquals(receivedInventoryStatusReadOnly, docketLine.WE_PalletIDInfo.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.ConsigneeNameOrPKInfo.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.SplitQuantityInfo.ReadOnly);
			AssertEquals(heldCodeReadonly, docketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_RequiredByDateInfo.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_CustomAttrib1Info.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_CustomAttrib2Info.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_CustomAttrib3Info.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_LineNoInfo.ReadOnly);
			AssertEquals(propertiesOtherThanDockDoorReadonly, docketLine.WE_SubLineNoInfo.ReadOnly);
		}

		#endregion

		#region TestSynchroniseOffsetsToWarehouseTime

		public void TestSynchroniseOffsetsToWarehouseTime_RequiredByDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);

			var warehouse = data.Whs1;
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			receiveLine.WE_RequiredByDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));
			receiveLine.SynchroniseOffsetsToWarehouseTime(calculationTimeZone);

			Assert("DocketLine should not be in error.", !receiveLine.HasErrors);
			var expectedOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				receiveLine.WE_RequiredByDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		public void TestSynchroniseOffsetsToWarehouseTime_RequiredByDate_FailsIfPropertyInError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);

			var warehouse = data.Whs1;
			var warehouseTimeZone = warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;
			var calculationTimeZone = warehouseTimeZone.GetCalculationTimeZone();
			var dateTime = new ZDateTime(2000, 06, 12, 12, 30, 00);

			receiveLine.WE_TransactionQuantity = -5m;
			receiveLine.WE_RequiredByDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			receiveLine.SynchroniseOffsetsToWarehouseTime(calculationTimeZone);

			Assert("Property should be in error.", receiveLine.WE_RequiredByDateInfo.HasErrors());
			AssertEquals("Do not sychronise Offset if Docket is in Error.",
				"12-Jun-2000 12:30 +00:00",
				receiveLine.WE_RequiredByDate.ToString("dd-MMM-yyyy hh:mm zzz"));
		}

		#endregion

		#region TestOnFactorySaving_ChangeInventoryStatus_CreatesReceiveLine

		public void TestOnFactorySaving_ChangeInventoryStatus_CreatesReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			Factory.Save(); // to generate ReceiveLine
			AssertEquals("Precondition - only 1 inventory should be created.", 1, receive.Inventory.Count);
			AssertEquals("Precondition - only 1 receive line should be created.", 1, receive.Lines.Count);

			var receiveLineOriginal = receive.Lines[0];
			var inventoryOriginal = receive.Inventory[0];
			receiveLineOriginal.HeldCodeChangeQuantity = 2m;
			receiveLineOriginal.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLineOriginal.IsInventoryEditForm = true;
			Factory.Save(); // to split inventory
			AssertEquals("New inventory record should not be displayed in inventories collection.", 1, receive.Inventory.Count);
			AssertEquals("New receive line record should not be displayed in lines collection.", 1, receive.Lines.Count);
			var inventories1 = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, receive.PK));
			var receiveLines1 = Factory.Load<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK));
			AssertEquals("New inventory record should be linked to docket job.", 2, inventories1.Length);
			AssertEquals("New receive line record should be linked to docket job.", 2, receiveLines1.Length);

			var receiveLineNew = receiveLines1.Single(l => !l.WE_IsOriginalInventory);
			var inventoryNew = inventories1.Cast<WhsInventoryView>().Single(l => !l.WI_IsOriginalReceiptLine);
			AssertInventoryMatches(inventoryOriginal, inventoryNew, receiveLineNew, 2m, InventoryStatus.Codes.Held, string.Empty, InventoryHoldCodes.Codes.Damaged);
			AssertReceiveLineMatches(receiveLineOriginal, receiveLineNew, 2m, InventoryStatus.Codes.Available, InventoryStatus.Codes.Held, string.Empty, InventoryHoldCodes.Codes.Damaged);

			// Functionally, in the same factory two receive lines can not both have the IsInventoryEditForm boolean set to true. Set to false to prevent HasChange being set and splitting again on receiveLineOriginal
			receiveLineOriginal.IsInventoryEditForm = false;

			receiveLineNew.HeldCodeChangeQuantity = 2m;
			receiveLineNew.HeldCodeToChangeTo = InventoryStatus.Codes.Held;
			receiveLineNew.IsInventoryEditForm = true;
			Factory.Save(); // to change hold code

			AssertEquals("No new inventory should be created.", 1, receive.Inventory.Count);
			AssertEquals("No new receive lines should be created.", 1, receive.Lines.Count);
			var inventories2 = Factory.Load<WhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WD, receive.PK));
			var receiveLines2 = Factory.Load<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK));
			AssertEquals("No new inventory record should be linked to docket job.", 2, inventories2.Length);
			AssertEquals("No new receive line record should be linked to docket job.", 2, receiveLines2.Length);

			var receiveLineNew2 = receiveLines1.Single(l => !l.WE_IsOriginalInventory);
			var inventoryNew2 = inventories1.Cast<WhsInventoryView>().Single(l => !l.WI_IsOriginalReceiptLine);
			AssertEquals("WI_TotalUnits", 2m, inventoryNew2.WI_TotalUnits);
			AssertEquals("WI_InventoryStatus", InventoryStatus.Codes.Held, inventoryNew2.WI_InventoryStatus);
			AssertEquals("WE_TransactionQuantity", 2m, receiveLineNew2.WE_TransactionQuantity);
			AssertEquals("WE_OriginalInventoryStatus", InventoryStatus.Codes.Available, receiveLineNew2.WE_OriginalInventoryStatus);
			AssertEquals("WE_WHC_NKOriginalInventoryHeldCode", string.Empty, receiveLineNew2.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("WE_CurrentInventoryStatus", InventoryStatus.Codes.Held, receiveLineNew2.WE_CurrentInventoryStatus);
			AssertEquals("WE_WHC_NKCurrentInventoryHeldCode", InventoryHoldCodes.Codes.Held, receiveLineNew2.WE_WHC_NKCurrentInventoryHeldCode);
		}

		void AssertInventoryMatches(WhsInventoryView inventoryOriginal, WhsInventoryView inventoryNew, WhsDocketLine receiveLineNew, ZDecimal expectedUnits, ZString expectedInventoryStatus, string expectedOriginalHeldCode = "", string expectedCurrentHeldCode = "")
		{
			AssertEquals("WI_WL", inventoryOriginal.WI_WL, inventoryNew.WI_WL);
			AssertEquals("WI_OP", inventoryOriginal.WI_OP, inventoryNew.WI_OP);
			AssertEquals("WI_PackingDate", inventoryOriginal.WI_PackingDate, inventoryNew.WI_PackingDate);
			AssertEquals("WI_ExpiryDate", inventoryOriginal.WI_ExpiryDate, inventoryNew.WI_ExpiryDate);
			AssertEquals("WI_ArrivalDate", inventoryOriginal.WI_ArrivalDate, inventoryNew.WI_ArrivalDate);
			AssertEquals("WI_PartAttrib1", inventoryOriginal.WI_PartAttrib1, inventoryNew.WI_PartAttrib1);
			AssertEquals("WI_PartAttrib2", inventoryOriginal.WI_PartAttrib2, inventoryNew.WI_PartAttrib2);
			AssertEquals("WI_PartAttrib3", inventoryOriginal.WI_PartAttrib3, inventoryNew.WI_PartAttrib3);
			AssertEquals("WI_BondedEntryKey", inventoryOriginal.WI_BondedEntryKey, inventoryNew.WI_BondedEntryKey);
			AssertEquals("WI_PalletID", inventoryOriginal.WI_PalletID, inventoryNew.WI_PalletID);
			AssertEquals("WI_WE_OriginalInDocketLineForRating", inventoryOriginal.WI_WE_OriginalInDocketLineForRating, inventoryNew.WI_WE_OriginalInDocketLineForRating);
			AssertEquals("WI_InDocketLineType", inventoryOriginal.WI_InDocketLineType, inventoryNew.WI_InDocketLineType);
			AssertEquals("WI_F3_NKPackType", inventoryOriginal.WI_F3_NKPackType, inventoryNew.WI_F3_NKPackType);
			AssertEquals("WI_OH_Client", inventoryOriginal.WI_OH_Client, inventoryNew.WI_OH_Client);
			AssertEquals("WI_WD", inventoryOriginal.WI_WD, inventoryNew.WI_WD);

			AssertEquals("WI_TotalUnits", expectedUnits, inventoryNew.WI_TotalUnits);
			AssertEquals("WI_IsOriginalReceiptLine", false, inventoryNew.WI_IsOriginalReceiptLine);
			AssertEquals("WI_WE_InDocketLine", receiveLineNew.PK, inventoryNew.WI_WE_InDocketLine);
			AssertEquals("WI_InDocketLineUnits", 0m, inventoryNew.WI_InDocketLineUnits);
			AssertEquals("WI_InventoryStatus", expectedInventoryStatus, inventoryNew.WI_InventoryStatus);
			AssertEquals("OriginalInventoryHeldCode", expectedOriginalHeldCode, inventoryNew.OriginalInventoryHeldCode);
			AssertEquals("WI_HeldCode", expectedCurrentHeldCode, inventoryNew.WI_HeldCode);
		}

		void AssertReceiveLineMatches(WhsReceiveLine receiveLineOriginal, WhsReceiveLine receiveLineNew, ZDecimal expectedUnits, ZString expectedOriginalInventoryStatus, ZString expectedCurrentInventoryStatus, string expectedOriginalHeldCode = "", string expectedCurrentHeldCode = "")
		{
			AssertEquals("WE_LineNo", receiveLineOriginal.WE_LineNo, receiveLineNew.WE_LineNo);
			AssertEquals("WE_SubLineNo", receiveLineOriginal.WE_SubLineNo + 1, receiveLineNew.WE_SubLineNo);
			AssertEquals("WE_OP", receiveLineOriginal.WE_OP, receiveLineNew.WE_OP);
			AssertEquals("WE_F3_NKPackType", receiveLineOriginal.WE_F3_NKPackType, receiveLineNew.WE_F3_NKPackType);
			AssertEquals("WE_ClientOrderedUnits", receiveLineOriginal.WE_ClientOrderedUnits, receiveLineNew.WE_ClientOrderedUnits);
			AssertEquals("WE_PackageGroupId", receiveLineOriginal.WE_PackageGroupId, receiveLineNew.WE_PackageGroupId);
			AssertEquals("WE_PerPackageQty", receiveLineOriginal.WE_PerPackageQty, receiveLineNew.WE_PerPackageQty);
			AssertEquals("WE_ExpiryDate", receiveLineOriginal.WE_ExpiryDate, receiveLineNew.WE_ExpiryDate);
			AssertEquals("WE_PartAttrib1", receiveLineOriginal.WE_PartAttrib1, receiveLineNew.WE_PartAttrib1);
			AssertEquals("WE_PartAttrib2", receiveLineOriginal.WE_PartAttrib2, receiveLineNew.WE_PartAttrib2);
			AssertEquals("WE_PartAttrib3", receiveLineOriginal.WE_PartAttrib3, receiveLineNew.WE_PartAttrib3);
			AssertEquals("WE_BondedEntryKey", receiveLineOriginal.WE_BondedEntryKey, receiveLineNew.WE_BondedEntryKey);
			AssertEquals("WE_PackingDate", receiveLineOriginal.WE_PackingDate, receiveLineNew.WE_PackingDate);
			AssertEquals("WE_AdjustmentArrivalDate", receiveLineOriginal.WE_AdjustmentArrivalDate, receiveLineNew.WE_AdjustmentArrivalDate);
			AssertEquals("WE_RecommendedUnitPrice", receiveLineOriginal.WE_RecommendedUnitPrice, receiveLineNew.WE_RecommendedUnitPrice);
			AssertEquals("WE_RX_NKUnitPriceCurrency", receiveLineOriginal.WE_RX_NKUnitPriceCurrency, receiveLineNew.WE_RX_NKUnitPriceCurrency);
			AssertEquals("WE_UnitDiscountPercent", receiveLineOriginal.WE_UnitDiscountPercent, receiveLineNew.WE_UnitDiscountPercent);
			AssertEquals("WE_UnitDiscountAmount", receiveLineOriginal.WE_UnitDiscountAmount, receiveLineNew.WE_UnitDiscountAmount);
			AssertEquals("WE_UnitPriceAfterDiscount", receiveLineOriginal.WE_UnitPriceAfterDiscount, receiveLineNew.WE_UnitPriceAfterDiscount);
			AssertEquals("WE_ExtendedLinePrice", receiveLineOriginal.WE_ExtendedLinePrice, receiveLineNew.WE_ExtendedLinePrice);
			AssertEquals("WE_DocketLineStatus", receiveLineOriginal.WE_DocketLineStatus, receiveLineNew.WE_DocketLineStatus);
			AssertEquals("WE_FinalisedDate", receiveLineOriginal.WE_FinalisedDate, receiveLineNew.WE_FinalisedDate);
			AssertEquals("WE_PalletID", receiveLineOriginal.WE_PalletID, receiveLineNew.WE_PalletID);
			AssertEquals("WE_ReceiveCrossDockOrderNo", receiveLineOriginal.WE_ReceiveCrossDockOrderNo, receiveLineNew.WE_ReceiveCrossDockOrderNo);
			AssertEquals("WE_RequiredByDate", receiveLineOriginal.WE_RequiredByDate, receiveLineNew.WE_RequiredByDate);
			AssertEquals("WE_LineComment", receiveLineOriginal.WE_LineComment, receiveLineNew.WE_LineComment);
			AssertEquals("WE_ReasonCode", receiveLineOriginal.WE_ReasonCode, receiveLineNew.WE_ReasonCode);
			AssertEquals("WE_CustomAttrib1", receiveLineOriginal.WE_CustomAttrib1, receiveLineNew.WE_CustomAttrib1);
			AssertEquals("WE_CustomAttrib2", receiveLineOriginal.WE_CustomAttrib2, receiveLineNew.WE_CustomAttrib2);
			AssertEquals("WE_CustomAttrib3", receiveLineOriginal.WE_CustomAttrib3, receiveLineNew.WE_CustomAttrib3);
			AssertEquals("WE_CustomAttrib4", receiveLineOriginal.WE_CustomAttrib4, receiveLineNew.WE_CustomAttrib4);
			AssertEquals("WE_CustomAttrib5", receiveLineOriginal.WE_CustomAttrib5, receiveLineNew.WE_CustomAttrib5);
			AssertEquals("WE_CustomAttrib6", receiveLineOriginal.WE_CustomAttrib6, receiveLineNew.WE_CustomAttrib6);
			AssertEquals("WE_CustomTextBlob1", receiveLineOriginal.WE_CustomTextBlob1, receiveLineNew.WE_CustomTextBlob1);
			AssertEquals("WE_CustomFlag1", receiveLineOriginal.WE_CustomFlag1, receiveLineNew.WE_CustomFlag1);
			AssertEquals("WE_CustomFlag2", receiveLineOriginal.WE_CustomFlag2, receiveLineNew.WE_CustomFlag2);
			AssertEquals("WE_CustomFlag3", receiveLineOriginal.WE_CustomFlag3, receiveLineNew.WE_CustomFlag3);
			AssertEquals("WE_CustomFlag4", receiveLineOriginal.WE_CustomFlag4, receiveLineNew.WE_CustomFlag4);
			AssertEquals("WE_CustomFlag5", receiveLineOriginal.WE_CustomFlag5, receiveLineNew.WE_CustomFlag5);
			AssertEquals("WE_CustomDate1", receiveLineOriginal.WE_CustomDate1, receiveLineNew.WE_CustomDate1);
			AssertEquals("WE_CustomDate2", receiveLineOriginal.WE_CustomDate2, receiveLineNew.WE_CustomDate2);
			AssertEquals("WE_CustomDate3", receiveLineOriginal.WE_CustomDate3, receiveLineNew.WE_CustomDate3);
			AssertEquals("WE_CustomDate4", receiveLineOriginal.WE_CustomDate4, receiveLineNew.WE_CustomDate4);
			AssertEquals("WE_CustomDate5", receiveLineOriginal.WE_CustomDate5, receiveLineNew.WE_CustomDate5);
			AssertEquals("WE_CustomDecimal1", receiveLineOriginal.WE_CustomDecimal1, receiveLineNew.WE_CustomDecimal1);
			AssertEquals("WE_CustomDecimal2", receiveLineOriginal.WE_CustomDecimal2, receiveLineNew.WE_CustomDecimal2);
			AssertEquals("WE_CustomDecimal3", receiveLineOriginal.WE_CustomDecimal3, receiveLineNew.WE_CustomDecimal3);
			AssertEquals("WE_CustomDecimal4", receiveLineOriginal.WE_CustomDecimal4, receiveLineNew.WE_CustomDecimal4);
			AssertEquals("WE_CustomDecimal5", receiveLineOriginal.WE_CustomDecimal5, receiveLineNew.WE_CustomDecimal5);
			AssertEquals("WE_WL", receiveLineOriginal.WE_WL, receiveLineNew.WE_WL);
			AssertEquals("WE_WD", receiveLineOriginal.WE_WD, receiveLineNew.WE_WD);
			AssertEquals("WE_PickGroup", receiveLineOriginal.WE_PickGroup, receiveLineNew.WE_PickGroup);
			AssertEquals("WE_GS_NKPutawayBy", receiveLineOriginal.WE_GS_NKPutawayBy, receiveLineNew.WE_GS_NKPutawayBy);
			AssertEquals("WE_PutawayTime", receiveLineOriginal.WE_PutawayTime, receiveLineNew.WE_PutawayTime);
			AssertEquals("WE_WL_TransferFrom", receiveLineOriginal.WE_WL_TransferFrom, receiveLineNew.WE_WL_TransferFrom);
			AssertEquals("WE_TransferFromPalletId", receiveLineOriginal.WE_TransferFromPalletId, receiveLineNew.WE_TransferFromPalletId);
			AssertEquals("WE_WE_OriginalDocketLineForRating", receiveLineOriginal.WE_WE_OriginalDocketLineForRating, receiveLineNew.WE_WE_OriginalDocketLineForRating);
			AssertEquals("WE_StockOnHand", expectedUnits, receiveLineNew.WE_StockOnHand);

			AssertEquals("WE_TransactionQuantity", expectedUnits, receiveLineNew.WE_TransactionQuantity);
			AssertEquals("WE_OriginalInventoryStatus", expectedOriginalInventoryStatus, receiveLineNew.WE_OriginalInventoryStatus);
			AssertEquals("WE_CurrentInventoryStatus", expectedCurrentInventoryStatus, receiveLineNew.WE_CurrentInventoryStatus);
			AssertEquals("WE_WHC_NKCurrentInventoryHeldCode", expectedCurrentHeldCode, receiveLineNew.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("WE_WHC_NKCurrentInventoryHeldCode", expectedOriginalHeldCode, receiveLineNew.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("WE_WE_ParentDocketLine", receiveLineOriginal.PK, receiveLineNew.WE_WE_ParentDocketLine);
			AssertEquals("WE_IsOriginalInventory", false, receiveLineNew.WE_IsOriginalInventory);
		}

		#endregion

		#region TestISerialSplittableLine Members

		#region TestISerialSplittableLine_IsSplittableProduct

		public void TestISerialSplittableLine_IsSplittableProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var notserialLine = (ISerialSplittableLine)Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			AssertEquals("Lines without a serial number should return false", false, notserialLine.IsSplittableProduct);

			var incompleteLine = (ISerialSplittableLine)Helper.CreateWhsReceiveLine(receive, ZGuid.Empty, 1m);
			AssertEquals("Lines with an empty product should return false", false, incompleteLine.IsSplittableProduct);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var serialisedLine = (ISerialSplittableLine)Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			AssertEquals("Lines with a serial attrib should return true", true, serialisedLine.IsSplittableProduct);
		}

		public void TestISerialSplittableLine_IsSplittableProduct_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var serialisedLine = (ISerialSplittableLine)Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			AssertEquals("Lines with a serial attrib should return false if Receive is pick by BOM.", false, serialisedLine.IsSplittableProduct);
		}

		#endregion

		#region TestISerialSplittableLine_IsFinalised

		public void TestISerialSplittableLine_IsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var line1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);
			var line2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			ISerialSplittableLine iSerialSplittableLine1 = line1;
			ISerialSplittableLine iSerialSplittableLine2 = line2;
			AssertEquals("Receive is finalised, Line should be considered Finalised.", true, iSerialSplittableLine1.IsFinalised);
			AssertEquals("Receive is not finalised, Line should not be considered Finalised.", false, iSerialSplittableLine2.IsFinalised);
		}

		#endregion

		#region TestISerialSplittableLine_Units

		public void TestISerialSplittableLine_Units()
		{
			var line = Factory.New<WhsReceiveLine>();
			line.WE_TransactionQuantity = 13.4m;

			ISerialSplittableLine iSerialSplittableLine = line;
			AssertEquals(13.4m, iSerialSplittableLine.Units);
		}

		#endregion

		#region TestSplitWhenSerialNumberExists

		public void TestSplitWhenSerialNumberExists_DoesntSplitInvalidLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var singleQuantityLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var noSerialLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			AssertEquals("Product should be splittable.", true, ((ISerialSplittableLine)singleQuantityLine).IsSplittableProduct);
			AssertEquals("Product should *not* be splittable.", false, ((ISerialSplittableLine)noSerialLine).IsSplittableProduct);

			// Make sure we dont split when serialised but quantity is wrong
			var splitLines = singleQuantityLine.SplitWhenSerialNumberExists();
			AssertEquals(2, receive.Lines.Count);
			AssertEquals(0, splitLines.Count());

			// Make sure we dont split when quantity is right but not serialised
			var splitLines2 = noSerialLine.SplitWhenSerialNumberExists();
			AssertEquals(2, receive.Lines.Count);
			AssertEquals(0, splitLines2.Count());
		}

		public void TestSplitWhenSerialNumberExists_SplitsCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var originalLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			originalLine.WE_SerialNumber = "SERIAL1";
			originalLine.WE_PartAttrib2 = "BATCH1";

			var splitLines = originalLine.SplitWhenSerialNumberExists();
			AssertEquals(4, splitLines.Count());
			AssertEquals(5, receive.Lines.Count);

			foreach (WhsReceiveLine line in receive.Lines)
			{
				AssertEquals("WI_ExpectedReceiptQuantity should be 1 after split", 1m, line.WE_ClientOrderedUnits);
				AssertEquals("WI_InDocketLineUnits should be 1 after split", 1m, line.WE_TransactionQuantity);
				AssertEquals("WI_TotalUnits should be 1 after split", 1m, line.WE_StockOnHand);
				AssertEquals("Non Serial Attribs should be cloned after split", "BATCH1", line.WE_PartAttrib2);

				if (line == originalLine)
				{
					AssertEquals("Serial Part Attrib should be same after split", "SERIAL1", line.WE_SerialNumber);
				}
				else
				{
					AssertEquals("Serial Part Attrib should be empty after split", "", line.WE_SerialNumber);
				}
			}
		}

		public void TestSplitWhenSerialNumberExists_SplitsPacks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, finalise: false);
			var line = receive.Lines[0];
			line.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			line.WE_SerialNumber = "1";
			AssertEquals("Precondition: Qty.", 5m, line.WE_TransactionQuantity);
			AssertEquals("Precondition: Pack Qty.", 1m, line.WE_PackQuantity);

			var splitLines = line.SplitWhenSerialNumberExists();
			AssertEquals("Should have generated new lines.", 4, splitLines.Count());
			AssertEquals("Should have generated new lines.", 5, receive.Lines.Count);
			AssertEquals("Each line should be 0.2 of a pallet.", true, receive.Lines.All(l => l.WE_PackQuantity == 0.20m && l.WE_F3_NKPackType == Constants.PkgUnit.Pallet));
		}

		#endregion

		#endregion

		#region TestIWhsReceiveLine Members

		public void TestIWhsReceiveLine_Members()
		{
			var receiveLine = GetNewBusinessObject(GetNewWhsDocket());
			receiveLine.WE_RequiredByDate = ZDateTime.BrettsBirthday.ToOffset();

			var iReceiveLine = receiveLine as IWhsReceiveLine;
			AssertEquals(iReceiveLine.Inventory, receiveLine.Inventory[0]);
			AssertEquals(iReceiveLine.WE_RequiredByDate, receiveLine.WE_RequiredByDate);
		}

		#endregion

		#region IOrgSupplierPartCollectionDefaultsForNewChild Members

		public void TestIOrgSupplierPartCollectionDefaultsForNewChild_SetupSupplierPart()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			var area = Helper.CreateArea(data.Whs1, "AREA1", Environment.CodeLists.AreaTypes.Codes.FreeStore);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			// Do Not Setup Attribute 3
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			// Preconditions
			AssertEquals("P1", data.Part1.OP_PartNum);
			AssertEquals("", data.Part1.OP_RH_NKCommodityCode);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePackingDate);

			// When Called but data is empty
			data.Line111.SetupSupplierPart(data.Part1);
			AssertEquals("P1", data.Part1.OP_PartNum);
			AssertEquals("", data.Part1.OP_RH_NKCommodityCode);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePackingDate);
			AssertEquals(0, (WhsProduct.GetWhsProduct(data.Part1)).ParamsByWhsAndClient.Count);

			// Setup data
			data.Part1.OP_PartNum = ""; // clear to see if it will be setup.
			data.Line111.InDocketLine.WE_OP = ZGuid.Invalid;
			data.Line111.InDocketLine.ProductCode = "NEWP1";
			data.Line111.InDocketLine.CommodityCode = "CODE";
			data.Line111.InDocketLine.WE_PartAttrib1 = "PA1";
			data.Line111.InDocketLine.WE_PartAttrib2 = "PA2";
			data.Line111.InDocketLine.WE_PartAttrib3 = "PA3";
			data.Line111.InDocketLine.WE_ExpiryDate = ZDate.Today;
			data.Line111.InDocketLine.WE_PackingDate = ZDate.Today;

			// When Called but data is set
			((WhsReceiveLine)data.Line111.InDocketLine).SetupSupplierPart(data.Part1);
			AssertEquals("NEWP1", data.Part1.OP_PartNum);
			AssertEquals("CODE", data.Part1.OP_RH_NKCommodityCode);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, data.Part1.RelatedOrganisations[0].OU_UsePartAttrib3); // not setup on Client;
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(true, data.Part1.RelatedOrganisations[0].OU_UsePackingDate);
			AssertEquals(0, (WhsProduct.GetWhsProduct(data.Part1)).ParamsByWhsAndClient.Count);
		}

		#endregion

		#region TestInventoryStatusIsCorrectWhenNewLineAdded

		public void TestInventoryStatusIsCorrectWhenNewLineAdded()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			Assert(receive.WD_ArrivalDate.IsEmpty);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			AssertEquals(InventoryStatus.Codes.Pending, inventory1.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Pending, inventory1.InDocketLine.WE_OriginalInventoryStatus);

			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			AssertEquals(InventoryStatus.Codes.Arrived, inventory2.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Arrived, inventory2.InDocketLine.WE_OriginalInventoryStatus);

			inventory2.WI_WL = data.Whs1.DefaultLocation.PK;
			AssertEquals(InventoryStatus.Codes.Putaway, inventory2.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, inventory2.InDocketLine.WE_OriginalInventoryStatus);
		}

		#endregion

		#region TestPutawayTransferCalculatedProperties

		[TestDate(2018, 8, 7, 7, 30, 0)]
		public void TestPutawayTransferCalculatedProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var rec1InRec1ForPart1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation1, "A", 2m);
			var rec4InRec1ForPart2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 1m, nonDockDoorLocation, "B");
			rec4InRec1ForPart2.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", ZDateTimeOffset.Empty);
			var rec2InRec2ForPart2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 2m, nonDockDoorLocation, "C");
			rec2InRec2ForPart2.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 2m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals(true, rec1InRec1ForPart1.HasPutawayTransfer);
			AssertEquals(transfer, ((WhsReceiveLine)rec1InRec1ForPart1.InDocketLine).PutawayTransfer);
			AssertEquals(transfer.WD_DocketID, ((WhsReceiveLine)rec1InRec1ForPart1.InDocketLine).PutawayTransferID);
			AssertEquals(true, rec1InRec1ForPart1.HasPutawayTransfer);

			AssertEquals(false, rec2InRec2ForPart2.HasPutawayTransfer);
			AssertNull(rec2InRec2ForPart2.PutawayTransferLine);
			AssertNull(rec2InRec2ForPart2.PutawayTransfer);
			AssertEquals("", rec2InRec2ForPart2.PutawayTransferID);

			Assert("Precondition", !rec4InRec1ForPart2.HasPutawayTransfer);
			rec4InRec1ForPart2.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;
			Assert("The property HasPutawayTransfer should return true as long as DocketLineStatus is PFU.", rec4InRec1ForPart2.HasPutawayTransfer);
		}

		#endregion

		#region TestPutawayTransferID_NoInventory

		public void TestPutawayTransferID_NoInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveLineInOtherFactory = otherFactory.Load<WhsReceiveLine>(receiveLine.PK);

			receiveLine.Delete();
			Factory.Save();

			AssertNoExceptionThrown("There should be no exceptions thrown when PutawayTransferID is invoked.", () => AssertEquals(ZString.Empty, receiveLineInOtherFactory.PutawayTransferID));
		}

		#endregion

		#region TestDestLocation

		[TestDate(2018, 8, 7, 7, 30, 0)]
		public void TestDestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID = "PLT-123";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, palletID, 100m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, palletID, 100m);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("DestLocation should be from putaway transferLine.", nonDockDoorLocation.ToLocationString(), receive.Lines[0].DestLocation);
			AssertEquals("DestLocation should be from putaway transferLine.", nonDockDoorLocation, receive.Lines[0].DestinationLocation);
		}

		[TestDate(2018, 8, 7, 7, 30, 0)]
		public void TestDestLocation_IsEmptyStringWhenLocationIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID = "PLT-123";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, palletID, 100m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, palletID, 100m);
			transferLine.LocationString = null;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("DestLocation should be emptyString when transferLine has no LocationString.", "", receive.Lines[0].DestLocation);
			AssertEquals("DestLocation should be emptyString when transferLine has no LocationString.", null, receive.Lines[0].DestinationLocation);
		}

		public void TestDestLocation_DirectPutawayReceiveLineLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var palletID = "PLT-123";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, nonDockDoorLocation, palletID, "");
			Factory.Save();

			AssertEquals("DestLocation should match one from Direct Putaway ReceiveLine when no transfer created.", receiveLine.LocationString, receive.Lines[0].DestLocation);
			AssertEquals("DestLocation should match one from Direct Putaway ReceiveLine when no transfer created.", nonDockDoorLocation, receive.Lines[0].DestinationLocation);
		}

		public void TestDestLocation_ReceiveToDockDoorLocationWithNoTransferCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var palletID = "PLT-123";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, dockDoorLocation, palletID, "");
			Factory.Save();

			AssertEquals("DestLocation should be empty on a receive to a dock door location.", ZString.Empty, receive.Lines[0].DestLocation);
			AssertEquals("DestLocation should be empty on a receive to a dock door location.", null, receive.Lines[0].DestinationLocation);
		}

		#endregion

		#region TestOperationalActionsFieldVisibility

		protected override void TestOperationalActionsFieldVisibilityCore()
		{
			base.TestOperationalActionsFieldVisibilityCore();
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsReceiveLine).GetProperty(WhsReceiveLine.Schema.PutawayTransfer)));
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsReceiveLine).GetProperty(WhsReceiveLine.Schema.PutawayTransferLine)));
		}

		#endregion

		#region TestUpdateLastInventoryChangeDate

		[TestDate(2019, 3, 26, 13, 15, 10)]
		public void TestUpdateLastInventoryChangeDate()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var location = Factory.Load<WhsLocation>(receiveLine.WE_WL);
			AssertEquals("Should updated when create receive.", now, location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			receiveLine.WE_TransactionQuantity = 0;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Quantity is zero no change.", now, location.WLV_LastInventoryChangeDate);

			receiveLine.WE_TransactionQuantity = 10;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Quantity is not zero therefore it should update LastInventoryChangeDate.", now.AddMinutes(10), location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(20).ToDateTime();
			receiveLine.WE_WL = Guid.Empty;
			receiveLine.RunPreSaveValidation();
			AssertEquals("Should update LastInventoryChangeDate for original location.", now.AddMinutes(20), location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(25).ToDateTime();
			receiveLine.WE_WL = Guid.NewGuid(); //  invalid
			receiveLine.RunPreSaveValidation();
			AssertEquals("Should update based on original location .", now.AddMinutes(25), location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(28).ToDateTime();
			receiveLine.WE_WL = location.PK;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Location is same as original location should not update .", now.AddMinutes(25), location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(30).ToDateTime();
			receiveLine.WE_OP = data.Part2.PK;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Change Product should update LastInventoryChangeDate.", now.AddMinutes(30), location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(40).ToDateTime();
			var otherLocation = data.Whs1.Rows[0].Locations.First(l => !l.PK.Equals(location.PK));
			receiveLine.WE_WL = otherLocation.PK;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Change location should update LastInventoryChangeDate.", now.AddMinutes(40), location.WLV_LastInventoryChangeDate);
			AssertEquals("Change location should update LastInventoryChangeDate in new location.", now.AddMinutes(40), otherLocation.WLV_LastInventoryChangeDate);
		}

		[TestDate(2019, 3, 26, 13, 15, 10)]
		public void TestUpdateLastInventoryChangeDate_ZeroUnits_BeforeChange()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var location = Factory.Load<WhsLocation>(receiveLine.WE_WL);
			var otherLocation = data.Whs1.Rows[0].Locations.First(l => !l.PK.Equals(location.PK));

			AssertEquals("Should updated when create receive.", now, location.WLV_LastInventoryChangeDate);
			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			receiveLine.WE_TransactionQuantity = 0;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Quantity is zero no change.", now, location.WLV_LastInventoryChangeDate);

			receiveLine.WE_TransactionQuantity = 10;
			receiveLine.WE_WL = otherLocation.PK;
			receiveLine.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Change location should **not** update LastInventoryChangeDate when original quantity is zero.", now, location.WLV_LastInventoryChangeDate);
			AssertEquals("Change location should update LastInventoryChangeDate in new location.", now.AddMinutes(10), otherLocation.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(20).ToDateTime();
			receiveLine.WE_WL = location.PK;
			receiveLine.RunPreSaveValidation();
			AssertEquals("Change location should update LastInventoryChangeDate.", now.AddMinutes(20), location.WLV_LastInventoryChangeDate);
			AssertEquals("Change location should update LastInventoryChangeDate when original quantity is not zero.", now.AddMinutes(20), otherLocation.WLV_LastInventoryChangeDate);
		}

		[TestDate(2019, 3, 26, 13, 15, 10)]
		public void TestUpdateLastInventoryChangeDate_ZeroUnits_AfterChange()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			WhsRow row1A = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			Factory.Save();

			var location = Factory.Load<WhsLocation>(receiveLine.WE_WL);
			var otherLocation1 = row1A.Locations[0];
			var otherLocation2 = row1A.Locations[1];
			AssertEquals("Should updated when create receive.", now, location.WLV_LastInventoryChangeDate);
			AssertEquals("Does not have any stock.", ZDateTimeOffset.Empty, otherLocation1.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			receiveLine.WE_TransactionQuantity = 0;
			receiveLine.WE_WL = otherLocation1.PK;
			receiveLine.RunPreSaveValidation();
			AssertEquals("Change location should **not** update LastInventoryChangeDate when original quantity is not zero.", ZDateTimeOffset.Empty, otherLocation1.WLV_LastInventoryChangeDate);
			AssertEquals("Change location should update LastInventoryChangeDate when quantity change to zero.", now.AddMinutes(10), location.WLV_LastInventoryChangeDate);
			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(20).ToDateTime();
			receiveLine.WE_TransactionQuantity = 12;
			receiveLine.WE_WL = otherLocation2.PK;
			receiveLine.RunPreSaveValidation();
			AssertEquals("Change location should **not** update LastInventoryChangeDate when original quantity is zero.", ZDateTimeOffset.Empty, otherLocation1.WLV_LastInventoryChangeDate);
			AssertEquals("Change location should update LastInventoryChangeDate when quantity change is not zero.", now.AddMinutes(20), otherLocation2.WLV_LastInventoryChangeDate);
		}

		[TestDate(2019, 3, 26, 13, 15, 10)]
		public void TestUpdateLastInventoryChangeDate_ChangeLocationIsNotInDB()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines.Single();
			var location = Factory.Load<WhsLocation>(receiveLine.WE_WL);
			var otherLocation = data.Whs1.Rows[0].Locations.First(l => !l.PK.Equals(location.PK));

			AssertEquals("Should updated when create receive.", now, location.WLV_LastInventoryChangeDate);
			AssertEquals("Does not have any stock.", ZDateTimeOffset.Empty, otherLocation.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddMinutes(10).ToDateTime();
			receiveLine.WE_WL = otherLocation.PK;
			receiveLine.RunPreSaveValidation();
			AssertEquals("Change location when is not in db should not update original location", now.AddMinutes(10), otherLocation.WLV_LastInventoryChangeDate);
			AssertEquals("Change location when is not in db should not update original location.", now, location.WLV_LastInventoryChangeDate);
		}

		#endregion

		#region Customs ReceiveLine

		public void TestCustomsReceiveLine_CreatesCustomsDataOnPreSaveValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);

			receiveLine.RunPreSaveValidation();
			var errors = receive.NotificationsIncludingChildren.GetErrors().ToArray();
			AssertEquals("Should have 1 error", 1, errors.Length);
			AssertEquals("Should be Entry Number Message", "Error - WB_EntryKey: Entry Number is mandatory for Customs Jobs.", errors[0].Message);
		}

		public void TestCustomsData_LineTransactionQuantityAssignedToBondedWhsQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 10m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = (ZShort)1;
			customsData.WB_CustomsQty = 10m;

			AssertNotEquals(10m, customsData.WB_BondedWhsQty);
			receiveLine.RunPreSaveValidation();
			AssertEquals("WE_TransactionQuantity assigned to WB_BondedWhsQty on PreSaveValidation.", 10m, customsData.WB_BondedWhsQty);

			var cloneReceiveLine = (WhsDocketLine)receiveLine.Clone();
			cloneReceiveLine.WE_WD = receive.PK;
			cloneReceiveLine.WE_TransactionQuantity = 100m;
			cloneReceiveLine.RunPreSaveValidation();
			AssertEquals("WE_TransactionQuantity assigned to WB_BondedWhsQty as clone is an original inventory.", 100m, cloneReceiveLine.CustomsData.WB_BondedWhsQty);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			receiveLine.IsInventoryEditForm = true;
			receiveLine.HeldCodeChangeQuantity = 2m;
			receiveLine.HeldCodeToChangeTo = InventoryStatus.Codes.Held;
			Factory.Save();

			var clonedReceiveLineFromHoldCodeChange = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 2m));
			AssertEquals("Precondition: WB_BondedWhsQty.", 10m, clonedReceiveLineFromHoldCodeChange.CustomsData.WB_BondedWhsQty);

			clonedReceiveLineFromHoldCodeChange.WE_TransactionQuantity = 15m;
			AssertEquals("WB_BondedWhsQty remains the same.", 10m, clonedReceiveLineFromHoldCodeChange.CustomsData.WB_BondedWhsQty);
		}

		public void TestHeldCodeToChangeTo_WithCustomsData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = (ZShort)1;
			customsData.WB_CustomsQty = 10m;
			customsData.WB_ValueForDuty = 100m;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			var origTransactionQty = receiveLine.WE_TransactionQuantity;
			receiveLine.IsInventoryEditForm = true;
			receiveLine.HeldCodeChangeQuantity = 2m;
			receiveLine.HeldCodeToChangeTo = InventoryStatus.Codes.Held;
			Factory.Save();

			AssertEquals("Should have cleared properties used during Held Code Change.", 0m, receiveLine.HeldCodeChangeQuantity);
			AssertEquals("Should have cleared properties used during Held Code Change.", string.Empty, receiveLine.HeldCodeToChangeTo);

			var originalReceiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, origTransactionQty - 2m));
			var clonedReceiveLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_StockOnHand, 2m));

			AssertEquals("Original receiveLine WE_TransactionQuantity.", origTransactionQty, originalReceiveLine.WE_TransactionQuantity);
			AssertEquals("Original receiveLine WB_BondedWhsQty.", origTransactionQty, originalReceiveLine.CustomsData.WB_BondedWhsQty);

			AssertEquals("Cloned receiveLine WE_TransactionQuantity", 2m, clonedReceiveLine.WE_TransactionQuantity);
			AssertEquals("Cloned receiveLine WE_CurrentInventoryStatus", InventoryStatus.Codes.Held, clonedReceiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Cloned receiveLine WE_WHC_NKOriginalInventoryHeldCode", string.Empty, clonedReceiveLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("Cloned receiveLine WE_WHC_NKCurrentInventoryHeldCode", InventoryHoldCodes.Codes.Held, clonedReceiveLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Cloned receiveLine WE_IsOriginalInventory", false, clonedReceiveLine.WE_IsOriginalInventory);

			var clonedCustomsData = clonedReceiveLine.CustomsData;
			AssertEquals("Cloned receiveLine WB_BondedWhsQty", origTransactionQty, clonedCustomsData.WB_BondedWhsQty);
			AssertEquals("Cloned receiveLine WB_EntryKey", "KEY", clonedCustomsData.WB_EntryKey);
			AssertEquals("Cloned receiveLine WB_EntryLineNo", (ZShort)1, clonedCustomsData.WB_EntryLineNo);
			AssertEquals("Cloned receiveLine WB_CustomsQty", 10m, clonedCustomsData.WB_CustomsQty);
			AssertEquals("Cloned receiveLine WB_ValueForDuty", 100m, clonedCustomsData.WB_ValueForDuty);
		}

		#endregion

		#region ILineToPutaway Members

		public void TestILineToPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 12m, data.Whs1.FindLocation("A-1"), "PLT-1", "HEL");

			ILineToPutaway lineToPutaway = receiveLine;
			AssertEquals("lineToPutaway.CheckPalletIDExists", true, lineToPutaway.CheckPalletIDExists);
			AssertEquals("lineToPutaway.DocketPK", receive.PK, lineToPutaway.DocketPK);
			AssertEquals("lineToPutaway.InventoryStatus", InventoryStatus.Codes.Held, lineToPutaway.InventoryStatus);
			AssertEquals("lineToPutaway.InventoryHeldCode", InventoryHoldCodes.Codes.Held, lineToPutaway.InventoryHeldCode);
			AssertEquals("lineToPutaway.IsValidToPutaway", true, lineToPutaway.IsValidToPutaway);
			AssertEquals("lineToPutaway.Location", data.Whs1.FindLocation("A-1"), lineToPutaway.Location);
			AssertEquals("lineToPutaway.LocationPK", data.Whs1.FindLocation("A-1").PK, lineToPutaway.LocationPK);
			AssertEquals("lineToPutaway.PackType", "UNT", lineToPutaway.PackType);
			AssertEquals("lineToPutaway.PalletID", "PLT-1", lineToPutaway.PalletID);
			AssertEquals("lineToPutaway.Product", data.Part1, lineToPutaway.Product);
			AssertEquals("lineToPutaway.ProductPK", data.Part1.PK, lineToPutaway.ProductPK);
			AssertEquals("lineToPutaway.QuantityToPutaway", 12m, lineToPutaway.QuantityToPutaway);
			AssertEquals("lineToPutaway.WarehousePK", data.Whs1.PK, lineToPutaway.WarehousePK);
			AssertEquals("Total Units should be set to InDocketLine Units.", 12m, receiveLine.WE_StockOnHand);

			lineToPutaway.PackType = "PLT";
			AssertEquals("PLT", receiveLine.WE_F3_NKPackType);
			AssertEquals("lineToPutaway.PackQuantity", 12m, lineToPutaway.PackQuantity);

			receiveLine.WE_F3_NKPackType = "CTN";
			receiveLine.WE_TransactionQuantity = 12m;
			AssertEquals("CTN", lineToPutaway.PackType);
			AssertEquals("lineToPutaway.PackQuantity", 1m, lineToPutaway.PackQuantity);

			lineToPutaway.LocationPK = data.Whs1.FindLocation("A-2").PK;
			AssertEquals(data.Whs1.FindLocation("A-2").PK, receiveLine.WE_WL);

			receiveLine.WE_WL = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, lineToPutaway.LocationPK);
			AssertEquals("Is Valid to Putaway if Line is not reserved.", true, lineToPutaway.IsValidToPutaway);

			lineToPutaway.PalletID = "PLT-2";
			AssertEquals("Receive line pallet id is updated to PLT-2.", "PLT-2", receiveLine.WE_PalletID);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(receiveLine.Inventory[0]);
			AssertEquals("Precondition: Stock is reserved.", 2m, reservedPickLine.ReservedQuantity);

			AssertEquals("Is not Valid to Putaway if Line is reserved and location is empty.", false, lineToPutaway.IsValidToPutaway);

			lineToPutaway.LocationPK = data.Whs1.FindLocation("A-1").PK;
			AssertEquals("Is Valid to Putaway if Line is reserved and location is not empty.", true, lineToPutaway.IsValidToPutaway);
		}

		public void TestILineToPutaway_PutawayPalletId_WithPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, dockDoorLocation, "PLT-1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-1", 5m);
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition: Receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);

			ILineToPutaway lineToPutaway = receiveLine;
			lineToPutaway.PalletID = "PLT-2";

			AssertEquals("Putaway transfer line's pallet id is updated.", "PLT-2", putawayTransferLine.WE_PalletID);
			AssertEquals("Receive line's pallet id is not updated.", "PLT-1", receiveLine.WE_PalletID);
		}

		public void TestILineToPutaway_Splitting()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			ILineToPutaway inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			var splitLine = inventory.Split(2m);
			AssertEquals("Should have split the new quantity onto the new line.", 2m, splitLine.QuantityToPutaway);
			AssertEquals("Original Inventory should have the remaining quantity.", 3m, inventory.QuantityToPutaway);
		}

		public void TestSplitReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var testLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);

			bool isValidationSuspendedDuringSplit = false;
			testLine.WE_TransactionQuantityInfo.ValueChanged += (sender, e) => isValidationSuspendedDuringSplit = testLine.IsValidationSuspended;

			var resultLine = testLine.Split(2m);
			AssertEquals(2m, resultLine.WE_TransactionQuantity);
			AssertEquals(3m, testLine.WE_TransactionQuantity);
			AssertEquals("Validation should be suspended when splitting Receive.", true, isValidationSuspendedDuringSplit);
		}

		#endregion

		#region TestChangingHoldCodeShouldFireWorkflow

		public void TestChangingHoldCodeShouldFireWorkflow()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var receiveWorkflowItem = receive.WorkflowItems.AddNew();
			receiveWorkflowItem.P9_Type = Constants.Workflow.WorkflowTriggerType;
			receiveWorkflowItem.TriggerConditions.TriggerEventCode = Events.ChangeOfIdentifierCode;
			receiveWorkflowItem.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			receiveWorkflowItem.TriggerConditions.TriggerConditionValue = "TYP=HOLD CODE";

			var action = receiveWorkflowItem.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			line1.IsInventoryEditForm = true;
			line1.HeldCodeChangeQuantity = 5;
			line1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			AssertEquals("Precondition: Should be able to save via the form", true, line1.Inventory.HasChanges);
			Factory.Save();

			var queryForCID = new ZQuery(StmALogSchema.SL_Parent, receive.PK);
			queryForCID.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);

			var queryWTE = new ZQuery(StmALogSchema.SL_Parent, receiveWorkflowItem.PK);
			queryWTE.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

			AssertEquals("CID must be propogated to receive.", 1, Factory.Load<StmALog>(queryForCID).Length);
			AssertEquals("WTE must be created.", 1, Factory.Load<StmALog>(queryWTE).Length);

			line2.IsInventoryEditForm = true;
			line2.HeldCodeChangeQuantity = 5;
			line2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertEquals("Precondition", true, line2.Inventory.HasChanges);
			Factory.Save();
			AssertEquals("Another CID event must be propogated to receive.", 2, Factory.Load<StmALog>(queryForCID).Length);
			AssertEquals("Another WTE event must be created.", 2, Factory.Load<StmALog>(queryWTE).Length);

			line2.HeldCodeChangeQuantity = 3;
			line2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			AssertEquals("Precondition", true, line2.Inventory.HasChanges);
			Factory.Save();
			AssertEquals("Another CID event must be propogated to receive.", 3, Factory.Load<StmALog>(queryForCID).Length);
			AssertEquals("Another WTE event must be created.", 3, Factory.Load<StmALog>(queryWTE).Length);
		}

		#endregion

		#region TestIsPickedForUnload

		public void TestIsPickedForUnload()
		{
			var receiveLine = GetNewBusinessObject();
			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.PickedForUnload;
			Assert("IsPickedForUnload should be true if WE_DocketLineStatus is PFU.", receiveLine.IsPickedForUnload);

			receiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			Assert("IsPickedForUnload should be false if WE_DocketLineStatus is not PFU.", !receiveLine.IsPickedForUnload);
		}

		#endregion

		#region TestFinalisationWithOutOfRangeExpiryDate

		public void TestFinalisationWithOutOfRangeExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			var outOfRangeExpiryDate = new ZDate(1950, 01, 01);
			receiveLine.WE_ExpiryDate = outOfRangeExpiryDate;
			AssertHasError(receiveLine.WE_ExpiryDateInfo, $"The date '01-Jan-1950' is more than 10 years old and thus is not valid.");

			receiveLine.WE_ExpiryDate = ZDate.Today.AddYears(1);
			AssertNoErrors(receiveLine.WE_ExpiryDateInfo);

			receiveLine.WE_PalletID = "A";
			receiveLine.WE_WL = dockDoorLocation.PK;
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transferLine.WE_ExpiryDate = ZDate.Today.AddYears(1);
			transfer.RunPreSaveValidation();
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			AssertEquals("Precondition: receive line has putaway transfer.", true, receiveLine.HasPutawayTransfer);
			AssertEquals("Precondition: Expiry date is read-only", true, receiveLine.WE_ExpiryDateInfo.ReadOnly);
			receiveLine.WE_ExpiryDate = outOfRangeExpiryDate;
			AssertNoError(receiveLine.WE_ExpiryDateInfo, $"The date '01-Jan-1950' is more than 10 years old and thus is not valid.");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
		}

		public void TestFinalisationWithOutOfRangeExpiryDate_DirectPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Factory.Save();

			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, nonDockDoorLocation);
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			Factory.Save();

			var outOfRangeExpiryDate = new ZDate(1950, 01, 01);
			receiveLine.WE_ExpiryDate = outOfRangeExpiryDate;
			AssertHasError(receiveLine.WE_ExpiryDateInfo, $"The date '01-Jan-1950' is more than 10 years old and thus is not valid.");

			receiveLine.WE_ExpiryDate = ZDate.Today.AddYears(1);
			AssertNoErrors(receiveLine.WE_ExpiryDateInfo);
			Factory.Save();

			AssertEquals("Precondition: receive line has no putaway transfer.", false, receiveLine.HasPutawayTransfer);
			receiveLine.WE_ExpiryDate = outOfRangeExpiryDate;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, receive.IsFinalised);
			AssertHasError(receiveLine.WE_ExpiryDateInfo, $"The date '01-Jan-1950' is more than 10 years old and thus is not valid.");
		}

		#endregion

		#region TestICusAddInfoTypeSupporterMembers
		public void TestICusAddInfoTypeSupporterMembers()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			ICusAddInfoTypeSupporter supporter = receiveLine;
			var dictionary = supporter.GetCusAddInfoTypes();
			AssertEquals("supporter.GetCusAddInfoTypes()", 1, dictionary.Count);
			AssertEquals(ObjectFactory.GetType<IWarehouseCustomsAddInfo>(), dictionary[WhsReceiveLine.CusAddInfoTypeAttribute.Codes.WarehouseCustomsAddInfo]);
			AssertSame("supporter.Factory", Factory, supporter.Factory);
			AssertEquals("supporter.PK", receiveLine.PK, supporter.PK);
			var fetchStrategies = supporter.GetFetchStrategies().ToArray();
			AssertEquals(1, fetchStrategies.Length);
			AssertType(ObjectFactory.GetType<ICusAddInfoTypeSupporterFetchStrategy>(), fetchStrategies[0]);
		}
		#endregion

		#region TestOnProductChange_DefaultHoldCode

		public void TestOnProductChange_DefaultHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var org2 = Helper.CreateClient("Org2");
			var relationShip = Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
			var damagedInventoryHeldCode = Factory.Load<WhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			relationShip.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org2, data.Whs1, "R1", Notify);

			AssertEquals("Precondition: orgpart relation has default hold code.", damagedInventoryHeldCode.PK, relationShip.OU_WHC_DefaultInventoryHoldCode);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Default held code is assigned to line.", "DAM", receiveLine.WE_WHC_NKOriginalInventoryHeldCode);

			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			AssertEquals("Default held code can be overridden.", "LCC", receiveLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertNoErrors("Default held code can be overridden.", receiveLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);
		}

		public void TestOnProductChange_DefaultHoldCode_HoldCodeNotDefaulted_InvalidProduct()
		{
			TestOnProductChange_DefaultHoldCode_HoldCodeNotDefaultedCore(ZGuid.Invalid);
		}

		public void TestOnProductChange_DefaultHoldCode_HoldCodeNotDefaulted_NoProduct()
		{
			TestOnProductChange_DefaultHoldCode_HoldCodeNotDefaultedCore(ZGuid.Empty);
		}

		void TestOnProductChange_DefaultHoldCode_HoldCodeNotDefaultedCore(ZGuid productPK)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var org2 = Helper.CreateClient("Org2");
			var relationShip = Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
			var damagedInventoryHeldCode = Factory.Load<WhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			relationShip.OU_WHC_DefaultInventoryHoldCode = damagedInventoryHeldCode.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org2, data.Whs1, "R1", Notify);

			AssertEquals("Precondition: orgpart relation has default hold code.", damagedInventoryHeldCode.PK, relationShip.OU_WHC_DefaultInventoryHoldCode);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			AssertEquals("Default held code is assigned to line.", "DAM", receiveLine.WE_WHC_NKOriginalInventoryHeldCode);

			receiveLine.WE_OP = productPK;
			AssertEquals("Default held code is not cleared.", "DAM", receiveLine.WE_WHC_NKOriginalInventoryHeldCode);
		}

		public void TestOnProductChange_DefaultHoldCode_BlankDefaultHoldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var org2 = Helper.CreateClient("Org2");
			var relationShipProd1 = Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
			var relationShipProd2 = Helper.CreateProductClientRelationShip(org2, data.Part2, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			AssertEquals("Precondition: orgpart relation1 has no default hold code.", ZGuid.Empty, relationShipProd1.OU_WHC_DefaultInventoryHoldCode);
			AssertEquals("Precondition: orgpart relation2 has no default hold code.", ZGuid.Empty, relationShipProd2.OU_WHC_DefaultInventoryHoldCode);

			var receive = Helper.CreateWhsReceive(org2, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = "DAM";
			AssertEquals("Precondition", "DAM", receiveLine.WE_WHC_NKOriginalInventoryHeldCode);

			receiveLine.WE_OP = data.Part2.PK;
			AssertEquals("Default blank held code is assigned to line.", string.Empty, receiveLine.WE_WHC_NKOriginalInventoryHeldCode);
		}

		#endregion

		#region override

		protected override void AssertMaxLengthExceededCore(Exception exception)
		{
			Assert(exception is TargetInvocationException); // Uses reflection to update dbo.WhsInventoryView
			Assert(exception.InnerException is MaxLengthExceededException);
		}

		protected override void AssertUseChosenInventoryRowHasSetProperties(WhsInventoryView expected, WhsDocketLine actual)
		{
			base.AssertUseChosenInventoryRowHasSetProperties(expected, actual);
			AssertEquals("Pallet", expected.WI_PalletID, actual.WE_PalletID);
		}

		#endregion

		#region Implementation

		protected override bool ShouldSettingHeldCodeChangeStatusBeforeFinalised
		{
			// Inventory becomes held when the Receive is finalised
			get { return false; }
		}

		protected override string ExpectedDefaultInventoryStatus
		{
			get { return InventoryStatus.Codes.Pending; }
		}

		protected override FinalisableDocketHelper<WhsReceive> GetNewDocketHelper(BusinessObjectFactory factory)
		{
			return new FinalisableReceiveHelper(factory);
		}

		#region WhsReceiveLineWrapperStrategy

		public void TestBuildWrapperStrategyIsWeb()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				// Action
				var wrapper = GetNewBusinessObject().WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && !(wrapper is WhsReceiveLine));
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		public void TestBuildWrapperStrategyDefault()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;

				// Action
				var wrapper = GetNewBusinessObject().WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && wrapper is WhsReceiveLine);
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		#endregion

		#endregion
	}

	#region ComponentLineForAssemblyCollectionTest

	[TestedType(typeof(WhsReceiveLine.ComponentLineForAssemblyCollection))]
	class ComponentLineForAssemblyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsReceiveLine.ComponentLineForAssemblyCollection>
	{
		protected override WhsReceiveLine.ComponentLineForAssemblyCollection GetCollectionToTest()
		{
			return new WhsReceiveLine.ComponentLineForAssemblyCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var inventoryLine = Factory.New<WhsReceiveLine>();
			return new ComponentLineForAssembly(inventoryLine, 2m);
		}
	}

	#endregion
}
