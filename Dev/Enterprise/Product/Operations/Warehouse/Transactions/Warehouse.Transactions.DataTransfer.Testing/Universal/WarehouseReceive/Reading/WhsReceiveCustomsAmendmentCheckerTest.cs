using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsReceiveCustomsAmendmentCheckerTest : WhsTestCaseWithFactory
	{
		#region Docket changes

		public void TestDocketNotInDatabase_OKToAmend()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);

			AssertEquals("It is ok to amend receive which is not yet saved.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestDocketWithNoChanges_OKToAmend()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			Assert(!receive.HasChanges);
			AssertEquals("It is ok to amend receive which has no changes.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestWarehouseIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.WD_WW_Whs = ZGuid.NewZGuid();
			AssertEquals("Warehouse change must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestClientIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.WD_OH_Client = ZGuid.NewZGuid();
			AssertEquals("Client change must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestSupplierIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			receive.SupplierDocAddress.OrganisationPK = data.Org1.PK;
			AssertNotNull("Precondition", receive.Supplier);
			var org2 = Helper.CreateClient("CL2");
			Factory.Save();

			// setting up another supplier on receive
			receive.SupplierDocAddress.OrganisationPK = org2.PK;
			AssertEquals("Supplier change must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestExternalReferenceIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.WD_ExternalReference = "ZZZ123";
			AssertEquals("External reference change must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		#endregion

		#region Docket line changes

		public void TestNewDocketLineIsNotCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.AddNew();
			AssertEquals("Adding new line should be fine.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestLineDeletionIsNotCriticalChangeWhenStockIsNotCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.Lines.DeleteAll();
			AssertEquals("Deleting a line which is not committed should be OK.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestLineDeletionIsCriticalChangeWhenStockIsCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(order);

			receive.Lines.DeleteAll();
			AssertEquals("Deleting a line with stock committed to a pick must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestProductIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_OP = data.Part2.PK;
			AssertEquals("Changing product must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestBondedEntryKeyIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_BondedEntryKey = "zxc123";
			AssertEquals("Changing WE_BondedEntryKey must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPackTypeIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_F3_NKPackType = "BOX";
			AssertEquals("Changing WE_F3_NKPackType must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPartAttrib1CanBeChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.Lines.Single().WE_PartAttrib1 = "ZZZ";
			AssertEquals("Changing WE_PartAttrib1 for non-picked stock is OK.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPartAttrib1CanBeChangedIfLineHasNotBeenPicked()
		{
			AssertPartAttribCanBeChangedIfLineHasNotBeenPicked(WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestPartAttrib1IsCriticalChangeForPickedStockOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_PartAttrib1 = "ZZZ";
			AssertEquals("Changing WE_PartAttrib1 must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPartAttrib2CanBeChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.Lines.Single().WE_PartAttrib2 = "ZZZ";
			AssertEquals("Changing WE_PartAttrib2 for non-picked stock is OK.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPartAttrib2CanBeChangedIfLineHasNotBeenPicked()
		{
			AssertPartAttribCanBeChangedIfLineHasNotBeenPicked(WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestPartAttrib2IsCriticalChangeForPickedStockOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_PartAttrib2 = "ZZZ";
			AssertEquals("Changing WE_PartAttrib2 must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPartAttrib3CanBeChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.Lines.Single().WE_PartAttrib3 = "ZZZ";
			AssertEquals("Changing WE_PartAttrib3 for non-picked stock is OK.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPartAttrib3CanBeChangedIfLineHasNotBeenPicked()
		{
			AssertPartAttribCanBeChangedIfLineHasNotBeenPicked(WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestPartAttrib3IsCriticalChangeForPickedStockOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_PartAttrib3 = "ZZZ";
			AssertEquals("Changing WE_PartAttrib3 must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		void AssertPartAttribCanBeChangedIfLineHasNotBeenPicked(SchemaStringColumn partAttribColumn)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			inventory1.InDocketLine[partAttribColumn] = "ZZZ";
			AssertEquals($"Changing {partAttribColumn.Name} for non-picked stock is OK.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestSerialNumberCanBeChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			receive.Lines.Single().WE_SerialNumber = "ZZZ";
			AssertEquals("Changing WE_SerialNumber for non-picked stock is OK.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestSerialNumberCanBeChangedIfLineHasNotBeenPicked()
		{
			AssertPartAttribCanBeChangedIfLineHasNotBeenPicked(WhsDocketLineSchema.WE_SerialNumber);
		}

		public void TestSerialNumberIsCriticalChangeForPickedStockOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_SerialNumber = "ZZZ";
			AssertEquals("Changing WE_SerialNumber must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestExpiryDateIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_ExpiryDate = ZDate.Today;
			AssertEquals("Changing WE_ExpiryDate must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPackingDateIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_PackingDate = ZDate.Today;
			AssertEquals("Changing WE_PackingDate must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPackageGroupIdIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_PackageGroupId = "Z123";
			AssertEquals("Changing WE_PackageGroupId must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		public void TestPerPackageQtyIsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			receive.Lines.Single().WE_PerPackageQty = 8;
			AssertEquals("Changing WE_PerPackageQty must prevent amendment.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		#endregion

		#region TestIncreasingWE_TransactionQuantityIsFineAsLongAsWE_StockOnHandAreAligned

		public void TestIncreasingWE_TransactionQuantityIsFineAsLongAsWE_StockOnHandAreAligned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition: stock reduced by pick", 7m, receiveLine.WE_StockOnHand);

			receiveLine.WE_TransactionQuantity = 12;
			receiveLine.WE_StockOnHand = 8;
			AssertEquals("When increase of WE_TransactionQuantity and WE_StockOnHand are not consistent - amendment should be denied.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));

			receiveLine.WE_StockOnHand = 9;
			AssertEquals("When increase of WE_TransactionQuantity and WE_StockOnHand are aligned - amendment should be allowed.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		#endregion

		#region TestDecreasingWE_TransactionQuantityIsFineAsLongAsWE_StockOnHandAreAligned

		public void TestDecreasingWE_TransactionQuantityIsFineAsLongAsWE_StockOnHandAreAligned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition: stock reduced by pick", 7m, receiveLine.WE_StockOnHand);

			receiveLine.WE_TransactionQuantity = 8;
			receiveLine.WE_StockOnHand = 6;
			AssertEquals("When decrease of WE_TransactionQuantity and WE_StockOnHand are not consistent - amendment should be denied.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));

			receiveLine.WE_StockOnHand = 5;
			AssertEquals("When decrease of WE_TransactionQuantity and WE_StockOnHand are aligned - amendment should be allowed.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		#endregion

		#region TestDecreasingWE_TransactionQuantityBelowAvailableStockLevelsPreventsAnAmendment

		public void TestDecreasingWE_TransactionQuantityBelowAvailableStockLevelsPreventsAnAmendment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			var receiveLine = receive.Lines.Single();
			Factory.Save();

			var finalisedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var finalisedPick = Helper.CreatePickNew(finalisedOrder);
			finalisedOrder.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var nonFinalisedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2);
			var nonFinalisedPick = Helper.CreatePickNew(nonFinalisedOrder);
			Factory.Save();

			AssertIsFinalisedPrecondition(finalisedPick);
			Assert("Precondition", !nonFinalisedPick.IsFinalised);
			AssertEquals("Precondition: stock reduced by finalised pick", 7m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: available to pick qty consider non finalised pick", 5m, receiveLine.AvailableToPickQuantity);

			// 2 units committed
			receiveLine.WE_TransactionQuantity = 4;
			receiveLine.WE_StockOnHand = 1;
			AssertEquals("Precondition: WI_AvailableToPickQuantity becomes negative", -1m, receiveLine.AvailableToPickQuantity);
			AssertEquals("When decreasing stock below  WI_AvailableToPickQuantity - amendment should be denied.", false, GetAmendmendChecker().CanDoAnAmendment(receive, false));

			receiveLine.WE_TransactionQuantity = 5;
			receiveLine.WE_StockOnHand = 2;
			AssertEquals("Precondition: WI_AvailableToPickQuantity becomes zero", 0m, receiveLine.AvailableToPickQuantity);
			AssertEquals("Reducing WI_AvailableToPickQuantity to zero should be fine.", true, GetAmendmendChecker().CanDoAnAmendment(receive, false));
		}

		#endregion

		#region TestAllowNoLineChangesIfStockWithdrawn_AllowBondedAttributeChange

		public void TestAllowNoLineChangesIfStockWithdrawn_AllowBondedAttributeChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			var customsData = Helper.CreateCustomsData(receive.Lines.Single());
			Factory.Save();

			var finalisedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var finalisedPick = Helper.CreatePickNew(finalisedOrder);
			finalisedOrder.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var ignoreFields = new[] { WhsBondedWarehouseAttributeSchema.Constants.WB_ParentID, WhsBondedWarehouseAttributeSchema.Constants.WB_ParentTableCode, WhsBondedWarehouseAttributeSchema.Constants.WB_ZoneStatus };
			foreach (var property in customsData.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent && !ignoreFields.Contains(i.Name)))
			{
				property.Value = Helper.GetChangedValue(property);
				AssertEquals("Allow Bonded Attribute changes if AllowNoLineChangesIfStockWithdrawn is true.", true, GetAmendmendChecker().CanDoAnAmendment(receive, true));
			}
		}

		#endregion

		#region TestAllowNoLineChangesIfStockWithdrawn

		public void TestAllowNoLineChangesIfStockWithdrawn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var finalisedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var finalisedPick = Helper.CreatePickNew(finalisedOrder);
			finalisedOrder.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var ignoreFieldsThatCantChange = new[] { WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus, WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode };
			foreach (var property in receive.Lines.Single().ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent && !ignoreFieldsThatCantChange.Contains(i.Name)))
			{
				property.Value = Helper.GetChangedValue(property);
				AssertEquals("No line changes can occur if AllowNoLineChangesIfStockWithdrawn is true.", false, GetAmendmendChecker().CanDoAnAmendment(receive, allowNoLineChangesIfStockWithdrawn: true));
			}
		}

		#endregion

		#region Implementation

		static IWhsReceiveCustomsAmendmentChecker GetAmendmendChecker() => new WhsReceiveCustomsAmendmentChecker();

		#endregion
	}
}
