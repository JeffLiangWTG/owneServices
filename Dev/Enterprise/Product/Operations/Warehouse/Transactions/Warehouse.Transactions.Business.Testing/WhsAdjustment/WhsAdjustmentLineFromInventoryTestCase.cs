using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentLineFromInventoryTestCase : DocketLineFromInventoryHelperTest<WhsAdjustment, WhsAdjustmentLine>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AdjustmentLineFromInventoryHelper(new TestNotificationBuffer(), null));
		}

		#endregion

		protected override WhsAdjustment GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			return Helper.CreateWhsAdjustment(client, warehouse);
		}

		protected override WhsAdjustmentLine GetNewDocketLine(WhsAdjustment docket, OrgSupplierPart part, WhsLocation location)
		{
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(docket, part, 10m, location);
			adjustmentLine.RunPreSaveValidation();

			return adjustmentLine;
		}

		protected override void AssertDocketLinesForSetDocketLineFromInventory(WhsDocketLine docketLineWithoutExclude, WhsDocketLine docketLineWithExclude, decimal qtyOnInventory)
		{
			AssertEquals("Qty should be copied.", -qtyOnInventory, docketLineWithoutExclude.WE_TransactionQuantity);
			AssertEquals("Qty should not be copied when excluded.", 0m, docketLineWithExclude.WE_TransactionQuantity);
		}

		protected override void AssertDocketLineEqualsInventory_CustomsData(WhsBondedWarehouseAttribute inventoryCustomsData, WhsBondedWarehouseAttribute docketlineCustomsData)
		{
			AssertEquals("CustomsData.WB_EntryLineNo", inventoryCustomsData.WB_EntryLineNo, docketlineCustomsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey,", inventoryCustomsData.WB_EntryKey, docketlineCustomsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", inventoryCustomsData.WB_AddInfo, docketlineCustomsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", inventoryCustomsData.WB_BondedWhsQty, docketlineCustomsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", inventoryCustomsData.WB_BondedWhsUnitOfQty, docketlineCustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", inventoryCustomsData.WB_CustomsQty, docketlineCustomsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", inventoryCustomsData.WB_CustomsUnitOfQty, docketlineCustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_DeclarationReference", inventoryCustomsData.WB_DeclarationReference, docketlineCustomsData.WB_DeclarationReference);
			AssertEquals("CustomsData.WB_EntryDate", inventoryCustomsData.WB_EntryDate, docketlineCustomsData.WB_EntryDate);
			AssertEquals("CustomsData.WB_IsActive", inventoryCustomsData.WB_IsActive, docketlineCustomsData.WB_IsActive);
			AssertEquals("CustomsData.WB_ParentTableCode, Should not be copied.", WhsDocketLineSchema.Constants.Prefix, docketlineCustomsData.WB_ParentTableCode);
			AssertEquals("CustomsData.WB_RN_NKCountryOfOrigin", inventoryCustomsData.WB_RN_NKCountryOfOrigin, docketlineCustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("CustomsData.WB_RX_NKTILVCurrency", inventoryCustomsData.WB_RX_NKTILVCurrency, docketlineCustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("CustomsData.WB_TILV", inventoryCustomsData.WB_TILV, docketlineCustomsData.WB_TILV);
			AssertEquals("CustomsData.WB_ValueForDuty", inventoryCustomsData.WB_ValueForDuty, docketlineCustomsData.WB_ValueForDuty);
			AssertEquals("CustomsData.WB_WB_InwardsEntry", inventoryCustomsData.WB_WB_InwardsEntry, docketlineCustomsData.WB_WB_InwardsEntry);
			AssertEquals("CustomsData.WB_PrimaryPreference", inventoryCustomsData.WB_PrimaryPreference, docketlineCustomsData.WB_PrimaryPreference);
			AssertEquals("CustomsData.WB_CustomsSecondQuantity", inventoryCustomsData.WB_CustomsSecondQuantity, docketlineCustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsData.WB_CustomsSecondUnitQty", inventoryCustomsData.WB_CustomsSecondUnitQty, docketlineCustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomsData.WB_CustomsThirdQuantity", inventoryCustomsData.WB_CustomsThirdQuantity, docketlineCustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsData.WB_CustomsThirdUnitQty", inventoryCustomsData.WB_CustomsThirdUnitQty, docketlineCustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("CustomsData.WB_Tariff", inventoryCustomsData.WB_Tariff, docketlineCustomsData.WB_Tariff);
			AssertEquals("CustomsData.WB_OA_ManufacturerAddress", inventoryCustomsData.WB_OA_ManufacturerAddress, docketlineCustomsData.WB_OA_ManufacturerAddress);
			AssertEquals("CustomsData.WB_IsMainInwardsProcessedItem", false, docketlineCustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("CustomsData.WB_IsSecondaryInwardsProcessedItem", false, docketlineCustomsData.WB_IsSecondaryInwardsProcessedItem);
		}

		protected override DocketLineFromInventoryHelper<WhsAdjustmentLine> GetInventoryHelper(WhsDocket docket)
		{
			return new AdjustmentLineFromInventoryHelper(docket?.NotificationSubscriber, (WhsAdjustment)docket);
		}
	}
}
