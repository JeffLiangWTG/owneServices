using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsOrderCustomsAmendmentCheckerTest : WhsTestCaseWithFactory
	{
		#region Docket Changes

		public void TestDocketNotInDatabase_OKToAmend_HLDCode()
		{
			TestDocketNotInDatabase_OKToAmend_Core(true);
		}

		public void TestDocketNotInDatabase_OKToAmend_NotHLDCode()
		{
			TestDocketNotInDatabase_OKToAmend_Core(false);
		}

		void TestDocketNotInDatabase_OKToAmend_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			AssertEquals("It is ok to amend order which is not yet saved.", true, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
		}

		public void TestDocketWithNoChanges_OKToAmend_HLDCode()
		{
			TestDocketWithNoChanges_OKToAmend_Core(true);
		}

		public void TestDocketWithNoChanges_OKToAmend_NotHLDCode()
		{
			TestDocketWithNoChanges_OKToAmend_Core(false);
		}

		void TestDocketWithNoChanges_OKToAmend_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			Assert(!order.HasChanges);
			AssertEquals("It is ok to amend order which has no changes.", true, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
		}

		public void TestDocketUnfinalised_OKToAmend_HLDCode()
		{
			TestDocketUnfinalised_OKToAmend_Core(true);
		}

		public void TestDocketUnfinalised_OKToAmend_NotHLDCode()
		{
			TestDocketUnfinalised_OKToAmend_Core(false);
		}

		void TestDocketUnfinalised_OKToAmend_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: false, finalisePick: false, pickableDockets: order);
			Factory.Save();

			order.WD_ExternalReference = "Changess";
			Assert(order.HasChanges);
			AssertEquals("It is ok to amend unfinalised orders.", true, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
		}

		public void TestDocketAnyChange_IsCriticalChange_HLDCode()
		{
			TestDocketAnyChange_IsCriticalChange_Core(true);
		}

		public void TestDocketAnyChange_IsCriticalChange_NotHLDCode()
		{
			TestDocketAnyChange_IsCriticalChange_Core(false);
		}

		void TestDocketAnyChange_IsCriticalChange_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1);
			var finalisedPick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			CombineAssertions(() =>
			{
				var ignoreFieldsThatCantChange = new[] { WhsDocketSchema.Constants.WD_WP };
				foreach (var property in order.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent && !ignoreFieldsThatCantChange.Contains(i.Name)))
				{
					var previousValue = property.Value;
					var newValue = Helper.GetChangedValue(property);
					property.Value = newValue;

					AssertEquals(
						$"No docket changes allowed if order is finalised - {property.HumanReadableName} Previous: {previousValue} New: {newValue}",
						false,
						GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
					property.Value = previousValue;
					Factory.Save();
				}
			});
		}

		#endregion

		#region Docket Line Changes

		public void TestDocketLineAddingNewLine_IsCriticalChange_HLDCode()
		{
			TestDocketLineAddingNewLine_IsCriticalChange_Core(true);
		}

		public void TestDocketLineAddingNewLine_IsCriticalChange_NotHLDCode()
		{
			TestDocketLineAddingNewLine_IsCriticalChange_Core(false);
		}

		void TestDocketLineAddingNewLine_IsCriticalChange_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			var originalLinePK = order.Lines.Single().PK;
			Factory.Save();

			order.Lines.AddNew();
			AssertEquals("Adding new line should be wrong.", false, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { originalLinePK }), hldCode));
		}

		public void TestDocketLine_AnyChangeOtherThanOutwardsEntryKey_NotCustomsClearing_IsCriticalChange_HLDCode()
		{
			TestDocketLineAnyChangeOtherThanOutwardsEntryKey_NotCustomsClearing_IsCriticalChange_Core(true);
		}

		public void TestDocketLine_AnyChangeOtherThanOutwardsEntryKey_NotCustomsClearing_IsCriticalChange_NotHLDCode()
		{
			TestDocketLineAnyChangeOtherThanOutwardsEntryKey_NotCustomsClearing_IsCriticalChange_Core(false);
		}

		void TestDocketLineAnyChangeOtherThanOutwardsEntryKey_NotCustomsClearing_IsCriticalChange_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			var ignoreFieldsThatCantChange = new[] { WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus, WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode, WhsDocketLineSchema.Constants.WE_WD };
			foreach (var property in order.Lines.Single().ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent && !ignoreFieldsThatCantChange.Contains(i.Name)))
			{
				var previousValue = property.Value;
				var newValue = Helper.GetChangedValue(property);
				property.Value = newValue;
				AssertEquals($"Precondition: Property '{property.Name}' changed. OriginalValue={property.OriginalValue}, Value={property.Value}.", true, property.HasChanges);
				AssertEquals($"No line changes should be able to occur.", false, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));

				property.Value = previousValue;
				Factory.Save();
			}
		}

		public void TestDocketLine_AnyChangeOtherThanOutwardsEntryKey_CustomsClearing_IsCriticalChange_HLDCode()
		{
			TestDocketLineAnyChangeOtherThanOutwardsEntryKey_CustomsClearing_IsCriticalChange_Core(true);
		}

		public void TestDocketLine_AnyChangeOtherThanOutwardsEntryKey_CustomsClearing_IsCriticalChange_NotHLDCode()
		{
			TestDocketLineAnyChangeOtherThanOutwardsEntryKey_CustomsClearing_IsCriticalChange_Core(false);
		}

		void TestDocketLineAnyChangeOtherThanOutwardsEntryKey_CustomsClearing_IsCriticalChange_Core(bool hldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.Lines.Cast<WhsOrderLine>().First().CustomsClearingInProgress = true;
			Factory.Save();

			var ignoreFieldsThatCantChange = new[] { WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus, WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode, WhsDocketLineSchema.Constants.WE_WD };
			foreach (var property in order.Lines.Single().ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent && !ignoreFieldsThatCantChange.Contains(i.Name)))
			{
				var previousValue = property.Value;
				var newValue = Helper.GetChangedValue(property);
				property.Value = newValue;

				AssertEquals($"Precondition: Property '{property.Name}' changed. OriginalValue={property.OriginalValue}, Value={property.Value}.", true, property.HasChanges);
				AssertEquals($"No line changes should be able to occur.", false, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));

				property.Value = previousValue;
				Factory.Save();
			}
		}

		public void TestDocketLine_AddingOutwardsEntryKey_NotCustomsClearing_HLDCode_OKToAmend()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			var orderLine = order.Lines.Single();
			orderLine.CustomsData.WB_EntryKey = "DummyNumber-1";
			orderLine.CustomsData.WB_EntryLineNo = 105;
			AssertEquals(
				"Changing Line Outwards Entry Number ok when no custom clearing in process and Service Code is HLD.",
				true,
				GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { orderLine.PK }), true));
		}

		public void TestDocketLine_AddingOutwardsEntryKey_NotCustomsClearing_NotHLDCode_IsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			var orderLine = order.Lines.Single();
			orderLine.CustomsData.WB_EntryKey = "DummyNumber-1";
			orderLine.CustomsData.WB_EntryLineNo = 105;
			AssertEquals(
				"Changing Line Outwards Entry Number NOT ok when no custom clearing in process and Service Code is NOT HLD.",
				false,
				GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { orderLine.PK }), false));
		}

		public void TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_NotHLDCode_CustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: true, hldCode: false, isCustomsClearing: true);
		}

		public void TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_NotHLDCode_NotCustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: true, hldCode: false, isCustomsClearing: false);
		}

		public void TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_HLDCode_CustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: true, hldCode: true, isCustomsClearing: true);
		}

		public void TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_HLDCode_NotCustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: true, hldCode: true, isCustomsClearing: false);
		}

		public void TestDocketLine_ClearingOutwardsEntryKey_IsCriticalChange_NotHLDCode_CustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: false, hldCode: false, isCustomsClearing: true);
		}

		public void TestDocketLine_ClearingOutwardsEntryKey_IsCriticalChange_NotHLDCode_NotCustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: false, hldCode: false, isCustomsClearing: false);
		}

		public void TestDocketLine_ClearingOutwardsEntryKey_IsCriticalChange_HLDCode_CustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: false, hldCode: true, isCustomsClearing: true);
		}

		public void TestDocketLine_ClearingOutwardsEntryKey_IsCriticalChange_HLDCode_NotCustomsClearing()
		{
			TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(expectToUpdate: false, hldCode: true, isCustomsClearing: false);
		}

		void TestDocketLine_ReplacingOutwardsEntryKey_IsCriticalChange_Core(bool expectToUpdate, bool hldCode, bool isCustomsClearing)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);

			var orderLine = order.Lines.Cast<WhsOrderLine>().Single();
			orderLine.CustomsData.WB_EntryKey = "InitialNumber-1";
			orderLine.CustomsData.WB_EntryLineNo = 7;
			orderLine.CustomsClearingInProgress = isCustomsClearing;
			Factory.Save();

			AssertEquals("Precondition: OrderLine CustomsClearingInProgress is correct", isCustomsClearing, orderLine.CustomsClearingInProgress);

			orderLine.CustomsData.WB_EntryKey = expectToUpdate ? "NewNumber-7" : string.Empty;
			orderLine.CustomsData.WB_EntryLineNo = expectToUpdate ? (ZShort)107 : ZShort.Zero;
			AssertEquals("Replacing Line Outwards Entry Number NOT ok.", false, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(), hldCode));
		}

		public void TestDocketLine_AddingOutwardsEntryKey_CustomsClearing_HLDCode_IsCriticalChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.Lines.Cast<WhsOrderLine>().First().CustomsClearingInProgress = true;
			Factory.Save();

			var orderLine = order.Lines.Single();
			orderLine.CustomsData.WB_EntryKey = "DummyNumber-1";
			orderLine.CustomsData.WB_EntryLineNo = 105;
			AssertEquals(
				"Changing Line Outwards Entry Number NOT ok when custom clearing in process and HLD Service code set.",
				false,
				GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { orderLine.PK }), true));
		}

		public void TestDocketLine_AddingOutwardsEntryKey_CustomsClearing_NotHLDCode_OkToAmend()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			order.Lines.Cast<WhsOrderLine>().First().CustomsClearingInProgress = true;
			Factory.Save();

			var orderLine = order.Lines.Single();
			orderLine.CustomsData.WB_EntryKey = "DummyNumber-1";
			orderLine.CustomsData.WB_EntryLineNo = 105;
			AssertEquals(
				"Changing Line Outwards Entry Number ok when custom clearing in process and NOT HLD Service code.",
				true,
				GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { orderLine.PK }), false));
		}

		public void TestDocketLine_BondedWhsAttribute_HLDCode()
		{
			TestDocketLine_BondedWhsAttribute_Core(true);
		}

		public void TestDocketLine_BondedWhsAttribute_NotHLDCode()
		{
			TestDocketLine_BondedWhsAttribute_Core(false);
		}

		void TestDocketLine_BondedWhsAttribute_Core(bool hldCode)
		{
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("P1", client);
			var whs = Helper.CreateWarehouse("Whs", "A", 3, 1);
			whs.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var bondedArea = Helper.CreateArea(whs, "C", AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PickingArea = bondedArea.PK;

			var receive = Helper.CreateWhsReceive(client, whs);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, product, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
			orderLine.WE_LineNo = 1;
			var customsData = orderLine.CustomsData;
			customsData.WB_EntryKey = "123";
			orderLine.CustomsClearingInProgress = !hldCode;

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();

			AssertEquals("Precondition", 1, order.Lines.Count);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			foreach (var property in customsData.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(i => i.IsPersistent))
			{
				var previousValue = property.Value;
				var newValue = Helper.GetChangedValue(property);
				property.Value = newValue;

				if (WhsBondedAttributesWhichMayBeChanged.Contains(property.Name))
				{
					AssertEquals("Line changes can occur.", true, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
				}
				else
				{
					AssertEquals("No line changes can occur.", false, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
				}

				property.Value = previousValue;
				Factory.Save();
			}
		}

		public void TestDocketLine_BondedWhsAttribute_AdditionalAddInfo_HLDCode()
		{
			TestDocketLine_BondedWhsAttribute_AdditionalAddInfoCore(true);
		}

		public void TestDocketLine_BondedWhsAttribute_AdditionalAddInfo_NotHLDCode()
		{
			TestDocketLine_BondedWhsAttribute_AdditionalAddInfoCore(false);
		}

		void TestDocketLine_BondedWhsAttribute_AdditionalAddInfoCore(bool hldCode)
		{
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("P1", client);
			var whs = Helper.CreateWarehouse("Whs", "A", 3, 1);
			whs.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var bondedArea = Helper.CreateArea(whs, "C", AreaTypes.Codes.Bonded);
			locationA1.WLV_WA_PickingArea = bondedArea.PK;

			var receive = Helper.CreateWhsReceive(client, whs);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, product, 10m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
			orderLine.WE_LineNo = 1;
			var customsData = orderLine.CustomsData;
			customsData.WB_EntryKey = "123";
			orderLine.CustomsClearingInProgress = !hldCode;

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();

			AssertEquals("Precondition", 1, order.Lines.Count);

			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var addInfo = Factory.New<Integration.IWarehouseCustomsAttributeAddInfo>();
			addInfo.B7_ParentID = customsData.PK;
			addInfo.B7_ParentTableCode = customsData.TablePrefix;
			addInfo.B7_Type = "ABC";
			addInfo.B7_AddInfoData = "SomeData";

			AssertEquals("Line changes can occur.", true, GetAmendmendChecker().CanDoAnAmendment(order, new HashSet<ZGuid>(new[] { order.Lines.Single().PK }), hldCode));
		}

		string[] WhsBondedAttributesWhichMayBeChanged
			=> new[]
				{
						WhsBondedWarehouseAttribute.Schema.WB_EntryKey,
						WhsBondedWarehouseAttribute.Schema.WB_EntryLineNo,
						WhsBondedWarehouseAttribute.Schema.WB_RN_NKCountryOfOrigin,
						WhsBondedWarehouseAttribute.Schema.WB_RN_NKCountryOfDestination,
						WhsBondedWarehouseAttribute.Schema.WB_CustomsQty,
						WhsBondedWarehouseAttribute.Schema.WB_CustomsUnitOfQty,
						WhsBondedWarehouseAttribute.Schema.WB_ValueForDuty,
						WhsBondedWarehouseAttribute.Schema.WB_AddInfo,
						WhsBondedWarehouseAttribute.Schema.WB_Tariff,
						WhsBondedWarehouseAttribute.Schema.WB_InwardStyle,
						WhsBondedWarehouseAttribute.Schema.WB_InwardProcedure,
						WhsBondedWarehouseAttribute.Schema.WB_Remarks,
				};

		#endregion

		#region Implementation

		static IWhsOrderCustomsAmendmentChecker GetAmendmendChecker() => new WhsOrderCustomsAmendmentChecker();

		#endregion
	}
}
