using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class BondedAdjustmentCreatorTest : WhsTestCaseWithFactory
	{
		#region TestAdjustOut

		public void TestAdjustOut()
		{
			var data = GetNewDataWithInventoryInBond();
			Factory.Save();

			var receive1 = CreateFinalisedReceiveWithBondedEntryKeysInDB(data, 0);
			var adjustment1 = BondedAdjustmentCreator.AdjustOut(receive1, (error) => { });
			AssertAdjustment(receive1, adjustment1, 0);

			var receive2 = CreateFinalisedReceiveWithBondedEntryKeysInDB(data, 1);
			var adjustment2 = BondedAdjustmentCreator.AdjustOut(receive2, (error) => { });
			AssertAdjustment(receive2, adjustment2, 1);

			var receive3 = CreateFinalisedReceiveWithBondedEntryKeysInDB(data, 2);
			Factory.Save();
			var adjustment3 = BondedAdjustmentCreator.AdjustOut(receive3, (error) => { });
			AssertAdjustment(receive3, adjustment3, 2);
		}

		public void TestAdjustOut_WithSerial()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsBondedWarehouse = true;
			data.Whs1.WW_IsVirtualWarehouse = true;
			data.Whs1.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			Factory.Save();
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, "EntryNumber123-1");
			receiveLine.WE_SerialNumber = "SN1";
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (error) => { });
			AssertAdjustment(receive, adjustment, 0);
		}

		public void TestAdjustOut_WhenReceiveIsHeld()
		{
			var data = GetNewDataWithInventoryInBond();
			Factory.Save();

			var receive = CreateFinalisedReceiveWithBondedEntryKeysInDB(data, 0);
			foreach (var line in receive.Lines)
			{
				line.HeldCodeChangeQuantity = line.WE_TransactionQuantity;
				line.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				line.ChangeInventoryHeldCode(true);
			}
			Factory.Save();

			var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (error) => { });
			AssertAdjustment(receive, adjustment, 0, InventoryStatus.Codes.Held);
		}

		WhsReceive CreateFinalisedReceiveWithBondedEntryKeysInDB(TestDataForInventory data, byte splitNo)
		{
			var year = ZDateTime.Now.Year;

			var receive = Helper.CreateWhsReceive(data.Org2, data.Whs2, "External-Ref");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_ExternalReferenceSplit = splitNo;

			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "EntryNumber123-1");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, "EntryNumber123-2");
			var line3 = Helper.CreateWhsReceiveInventoryLine(
				receive, data.Part2, 3m, new ZDate(year, 5, 6), new ZDate(year - 1, 1, 1), "1", "2", "3", "EntryNumber123-3");

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			return receive;
		}

		static void AssertAdjustment(WhsReceive receive, WhsAdjustment adjustment, byte expectedAdjustmentSplitNo, string expectedHoldCode = "")
		{
			AssertAdjustment(receive, adjustment, expectedAdjustmentSplitNo, expectedHoldCode, negateLines: true);
		}

		#endregion

		#region TestAdjustOut_WithInvalidDocket

		public void TestAdjustOut_WithInvalidDocket_UnfinalisedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsBondedWarehouse = true;
			data.Whs1.WW_IsVirtualWarehouse = true;
			data.Whs1.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, "EntryNumber123-1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			AssertEquals(false, receive.IsFinalised);
			var errorMessage = string.Empty;
			var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (errorMsg) => errorMessage = errorMsg);

			AssertNull(adjustment);
			AssertEquals("Only Bonded Dockets that are Finalized (in virtual warehouse or for change of ownership) can be Adjusted out.", errorMessage);
		}

		public void TestAdjustOut_WithInvalidDocket_NotVirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, "EntryNumber123-1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var errorMessage = string.Empty;
			var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (errorMsg) => errorMessage = errorMsg);

			AssertNull(adjustment);
			AssertEquals("Only Bonded Dockets that are Finalized (in virtual warehouse or for change of ownership) can be Adjusted out.", errorMessage);
		}

		public void TestAdjustOut_WithInvalidDocket_NotCustomsReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, "EntryNumber123-1");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var errorMessage = string.Empty;
			var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (errorMsg) => errorMessage = errorMsg);

			AssertNull(adjustment);
			AssertEquals("Only Bonded Dockets that are Finalized (in virtual warehouse or for change of ownership) can be Adjusted out.", errorMessage);
		}

		#endregion

		#region TestAdjust_ResumesValidation

		public void TestAdjust_ResumesValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// make warehouse virtual
			data.Whs1.WW_IsBondedWarehouse = true;
			data.Whs1.WW_IsVirtualWarehouse = true;
			data.Whs1.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			Factory.Save();

			// receive 50 units
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, "123");
			Factory.Save();

			// steal the stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			order.Lines[0].WE_BondedEntryKey = "123";
			order.Lines[0].CustomsData.WB_EntryKey = "DummyOutward-1";
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				order.FinaliseDocketAlwaysFinalisingPick();
			}
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(order.Pick);

			// simulate import suspending validation
			using (new DisposableAction(() => Factory.SuspendValidation(), () => Factory.ResumeValidation()))
			{
				var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (error) => { });
				AssertEquals("Validation should prevent finalise as there is not enough stock to adjust out.", false, adjustment.IsFinalised);
			}
		}

		#endregion

		#region Implementation

		static void AssertAdjustment(WhsDocket docket, WhsAdjustment adjustment, byte expectedAdjustmentSplitNo, ZString expectedHoldCode, bool negateLines)
		{
			// header
			AssertEquals(docket.Warehouse, adjustment.Warehouse);
			AssertEquals(docket.Client, adjustment.Client);
			AssertEquals(docket.WD_ExternalReference, adjustment.WD_ExternalReference);
			AssertEquals(expectedAdjustmentSplitNo, adjustment.WD_ExternalReferenceSplit);
			AssertEquals(AdjustmentType.Codes.Customs, adjustment.WD_DocketSubType);
			AssertEquals("Adjustment should be Finalised.", true, adjustment.IsFinalised);

			// lines
			var multiplier = negateLines ? -1 : 1; // -1 adjusts out received stock
			AssertEquals(docket.Lines.Count, adjustment.Lines.Count);

			var expectedInventoryStatus = expectedHoldCode.IsEmpty ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Held;

			foreach (var line in docket.Lines)
			{
				var entryKeyAndNo = line.WE_BondedEntryKey.Split('-');
				var expectedEntryKey = entryKeyAndNo[0];
				var expectedEntryLineNo = ZShort.Parse(entryKeyAndNo[1]);

				// bit ugly but simpler than passing a delegate to AssertAdjustment
				var inventory = docket is WhsOrder ? ((WhsOrderLine)line).PickLines[0].Inventory : line.Inventory[0];

				AssertEquals(1,
					(from adjLine in adjustment.Lines
					 where
							 // critical
							 adjLine.WE_TransactionQuantity == line.WE_TransactionQuantity * multiplier
						&& adjLine.WE_OP == line.WE_OP
						&& adjLine.WE_BondedEntryKey == line.WE_BondedEntryKey
						&& adjLine.CustomsData.WB_EntryKey == expectedEntryKey
						&& adjLine.CustomsData.WB_EntryLineNo == expectedEntryLineNo
						&& adjLine.WE_PartAttrib1 == inventory.WI_PartAttrib1
						&& adjLine.WE_PartAttrib2 == inventory.WI_PartAttrib2
						&& adjLine.WE_PartAttrib3 == inventory.WI_PartAttrib3
						&& adjLine.WE_SerialNumber == inventory.WI_SerialNumber

						// less important
						&& adjLine.WE_ReasonCode == "AMD"
						&& adjLine.WE_OriginalInventoryStatus == expectedInventoryStatus
						&& adjLine.WE_WHC_NKOriginalInventoryHeldCode == expectedHoldCode
					 select adjLine).Count());
			}
		}

		TestDataForInventory GetNewDataWithInventoryInBond()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			data.Whs1.WW_IsBondedWarehouse = true;
			data.Whs1.WW_IsVirtualWarehouse = true;
			data.Whs1.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;

			data.Whs2.WW_IsBondedWarehouse = true;
			data.Whs2.WW_IsVirtualWarehouse = true;
			data.Whs2.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;

			return data;
		}

		#endregion
	}
}
