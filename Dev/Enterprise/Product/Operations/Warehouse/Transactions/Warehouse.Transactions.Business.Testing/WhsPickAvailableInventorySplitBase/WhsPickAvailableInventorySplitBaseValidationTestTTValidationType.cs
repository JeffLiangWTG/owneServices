using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsPickAvailableInventorySplitBaseValidationTest<T, TValidationType> : WhsBusinessObjectValidationTestCase
			where T : WhsPickAvailableInventorySplitBase
			where TValidationType : WhsPickAvailableInventorySplitBaseValidation
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(TValidationType), CreateAvailableInventory().Validation.AutoValidationType);
		}

		public void TestValidateAssignedToPK()
		{
			var staff = Helper.CreateGlbStaff("BRS", "BRS");
			var availableInventory = CreateAvailableInventoryWithPickLine();
			AssertNoErrors("Precondition", availableInventory.AssignedToPKInfo);

			availableInventory.PickedDate = ZDateTimeOffset.Today;
			availableInventory.AssignedToPK = ZGuid.Empty;
			AssertHasError(availableInventory.AssignedToPKInfo, "Lines that have been picked must have a Picker.");

			availableInventory.PickedDate = ZDateTimeOffset.Empty;
			AssertNoErrors("Should have no error after clearing PickedDate.", availableInventory.AssignedToPKInfo);

			availableInventory.PickedDate = ZDateTimeOffset.Today;
			availableInventory.AssignedToPK = ZGuid.Empty;
			AssertHasError(availableInventory.AssignedToPKInfo, "Lines that have been picked must have a Picker.");

			availableInventory.AssignedToPK = staff.PK;
			AssertNoErrors("Should have no error after clearing PickedDate.", availableInventory.AssignedToPKInfo);
		}

		public void TestValidateAssignedToPK_IsPicking()
		{
			var expectedErrorMessage = "Lines being Picked cannot be reassigned.";
			var user1 = Helper.CreateGlbStaff("XYZ", "XYZ");
			var user2 = Helper.CreateGlbStaff("JGU", "JGU");

			var availableInventory = CreateAvailableInventoryWithPickLine();
			AssertNoErrors("Precondition", availableInventory.AssignedToPKInfo);
			AssertNotNull("Precondition", availableInventory.PickLinesForPickingDetails);

			foreach (var pickLine in availableInventory.PickLinesForPickingDetails)
			{
				AssertEquals("Precondition:", false, pickLine.WZ_IsPicking);
				AssertEquals("Precondition:", "", pickLine.WZ_GS_NKAssignedTo);
			}

			availableInventory.AssignedToPK = user1.PK;
			AssertNoError("When PickLine is not Picking and reassign to new Picker should not cause error.", availableInventory.AssignedToPKInfo, expectedErrorMessage);
			Factory.Save();

			foreach (var pickLine in availableInventory.PickLinesForPickingDetails)
			{
				pickLine.WZ_IsPicking = true;
			}

			availableInventory.AssignedToPK = user1.PK;
			AssertNoError("When PickLine is Picking and assign to current Picker should not cause error.", availableInventory.AssignedToPKInfo, expectedErrorMessage);

			availableInventory.AssignedToPK = user2.PK;
			AssertHasError("When PickLine is Picking and re-assign to new Picker should cause error.", availableInventory.AssignedToPKInfo, expectedErrorMessage);

			availableInventory.AssignedToPK = ZGuid.Empty;
			AssertHasError("When PickLine is Picking and re-assign to empty Picker should cause error.", availableInventory.AssignedToPKInfo, expectedErrorMessage);
		}

		public abstract void TestValidatePickedDate();

		#region TestValidatePickedDate_InTransit

		public void TestValidatePickedDate_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories are picked.", 2, availableInventory.PickLines.Count()); // because of 2 inventories

			var pickLine = availableInventory.PickLines.First();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var newPickLine = order.Lines[0].PickLines.Single(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsValid);

			var inventorySplit = CreateAvailableInventory();
			SetDataOnInventorySplit(inventorySplit, availableInventory, new[] { PickLinePair.New(newPickLine) });
			inventorySplit.Validation.ValidatePickedDate();
			AssertHasError(inventorySplit.PickedDateInfo, "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked.");

			// fully release pick line
			newPickLine.WZ_ReleaseCapturedPartAttrib1 = "RED";

			inventorySplit.Validation.ValidatePickedDate();
			AssertNoErrors(inventorySplit.PickedDateInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll_PickedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories are picked.", 2, availableInventory.PickLines.Count()); // because of 2 inventories

			var inventorySplit = CreateAvailableInventory();
			SetDataOnInventorySplit(inventorySplit, availableInventory, availableInventory.PickLines.ToPickLinePairs());
			AssertNoErrors(inventorySplit.PickedDateInfo);

			using (inventorySplit.GetValidationSuspender())
			{
				inventorySplit.PickedDate = ZDateTimeOffset.Now;
				AssertNoErrors(inventorySplit.PickedDateInfo);
			}

			inventorySplit.Validation.ValidateAll();
			AssertHasError(inventorySplit.PickedDateInfo, "The number of Attributes that were Release Captured does not match the numbers of Units that were Picked.");
		}

		#region TestValidateAll_PickedDate_CustomsOrderAcceptEvent

		public void TestValidateAll_PickedDate_CustomsOrderAcceptEvent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			order.Logs.AddNew(Events.HoldTheWarehouseOrder);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			var inventorySplit = CreateAvailableInventory();
			SetDataOnInventorySplit(inventorySplit, availableInventory, availableInventory.PickLines.ToPickLinePairs());
			AssertNoErrors(inventorySplit.PickedDateInfo);

			inventorySplit.PickedDate = ZDateTimeOffset.Now;
			AssertHasError(inventorySplit.PickedDateInfo, "Cannot pick Customs orders on Hold, awaiting Customs response.");

			var allowFinaliseLog = order.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
			Helper.SetLogUTCTimeOnFactorySave(TestConnection, allowFinaliseLog, ZDateTime.Now.AddDays(1));
			Factory.Save(); // to set Posted Time

			inventorySplit.Validation.ValidateAll();
			AssertNoError(inventorySplit.PickedDateInfo, "Cannot pick Customs orders on Hold, awaiting Customs response.");
		}

		#endregion

		public void TestValidateAll_AssignedTo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Order is Picked.", 1, pick.OrderedInventories[0].AvailableInventories.Count);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition: Two Inventories are picked.", 2, availableInventory.PickLines.Count()); // because of 2 inventories

			var inventorySplit = CreateAvailableInventory();
			SetDataOnInventorySplit(inventorySplit, availableInventory, availableInventory.PickLines.ToPickLinePairs());
			AssertNoErrors(inventorySplit.AssignedToPKInfo);

			inventorySplit.PickedDate = ZDateTimeOffset.Now;
			AssertNoErrors(inventorySplit.AssignedToPKInfo);

			using (inventorySplit.GetValidationSuspender())
			{
				inventorySplit.AssignedToPK = ZGuid.Empty;
				AssertNoErrors(inventorySplit.AssignedToPKInfo);
			}

			inventorySplit.Validation.ValidateAll();
			AssertHasError(inventorySplit.AssignedToPKInfo, "Lines that have been picked must have a Picker.");
		}

		#endregion

		#region Implementation

		protected abstract void SetDataOnInventorySplit(T inventorySplit, WhsPickAvailableInventory availableInventory, IPickLinePair[] pickLines);

		protected abstract T CreateAvailableInventory();

		protected abstract T CreateAvailableInventoryWithPickLine();

		#endregion
	}
}
