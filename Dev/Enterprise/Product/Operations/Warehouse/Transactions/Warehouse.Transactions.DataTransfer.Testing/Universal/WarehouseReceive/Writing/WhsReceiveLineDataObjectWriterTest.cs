using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsReceiveLineDataObjectWriterTest : WhsDocketLineDataObjectWriterTest<WhsReceiveLine, WhsReceiveLineDataObjectWriter>
	{
		#region TestBasicInventoryLevelFieldMappings

		public void TestBasicInventoryLevelFieldMappings()
		{
			var receiveLine = GetReceiveLine(Factory);
			Factory.SaveForTesting();

			var inventoryDataObject = new WhsReceiveLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveLine))).GetDataObject(receiveLine);

			AssertNotNull("dataObject", inventoryDataObject);
			CombineAssertions(() => AssertContents(inventoryDataObject));
			AssertOrganizationBO_CRAHOLSYD("ConsigneeAddress", inventoryDataObject.Consignee, "ConsigneeAddress");
		}

		#endregion

		#region TestConsigneeAddressIsNullOnDataObjectIfWasEmptyAndCustomsDataIsNotCreatedIfInventoryLineHasNoCustomsData

		public void TestConsigneeAddressIsNullOnDataObjectIfWasEmptyAndCustomsDataIsNotCreatedIfInventoryLineHasNoCustomsData()
		{
			var inventory = Factory.NewWithValidTestData<WhsInventoryView>();
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;

			var inventoryDataObject = new WhsReceiveLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, inventory))).GetDataObject(receiveLine);

			AssertNotNull("dataObject", inventoryDataObject);
			AssertNull("dataObject.CustomsData", inventoryDataObject.CustomsData);
			AssertNull("dataObject.Consignee", inventoryDataObject.Consignee);
		}

		#endregion

		#region Implementation

		internal static WhsReceiveLine GetReceiveLine(UniversalObjectFactory factory)
		{
			var client = factory.NewWithValidTestData<OrgHeader>();

			var whsArea2 = factory.NewWithValidTestData<WhsArea>();
			whsArea2.WA_Name = "COOL";
			whsArea2.WA_AreaType = "BON";

			var whsReceive = factory.New<WhsReceive>();
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ROLLDOLL";
			product.OP_Desc = "Roll this doll on the soil";
			product.OP_StockKeepingUnit = "NO";
			product.OP_RH_NKCommodityCode = "HAZ";

			var inventory = whsReceive.Lines.AddNew().Inventory[0];
			inventory.WI_OH_Client = client.PK;
			whsReceive.WD_WW_Whs = whsArea2.WA_WW_Whs;
			whsReceive.WD_OH_Client = client.PK;
			((WhsReceiveLine)inventory.InDocketLine).ConsigneeDocAddress.E2_OA_Address = GetOrganizationBO_CRAHOLSYD(factory.BOFactory).MainAddress.PK;
			inventory.WI_OP = product.PK;

			inventory.WI_ExpiryDate = new ZDate(2011, 1, 1);
			inventory.WI_F3_NKPackType = "CTN";
			inventory.WI_ExpectedReceiptQuantity = 11.1m;
			inventory.WI_InDocketLineUnits = 22.2m;
			inventory.WI_LineNo = new ZShort(3);
			inventory.WI_PackingDate = new ZDate(2011, 1, 2);
			inventory.WI_PalletID = "PALLET1";
			inventory.WI_PartAttrib1 = "Wide";
			inventory.WI_PartAttrib2 = "Long";
			inventory.WI_PartAttrib3 = "Thick";
			inventory.WI_SerialNumber = "SERN";
			inventory.WI_ReceiveCrossDockOrderNo = "REC1";
			inventory.WI_SplitQuantity = 44.4m;
			inventory.WI_SubLineNo = new ZShort(4);
			inventory.WI_UnitsUQ = "NO";
			inventory.WI_WD = whsReceive.PK;
			inventory.WI_InventoryStatus = "PND";
			inventory.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			inventory.InDocketLine.WE_CurrentHoldReason = "Washed my car";

			inventory.WI_BondedEntryKey = "The Key to my Heart";
			inventory.WI_WE_OriginalInDocketLineForRating = whsReceive.Lines[0].PK;

			var helper = new WhsTestHelperFunctions(factory.BOFactory);
			helper.CreateProductUnit(product, "CTN", 1000);

			factory.SaveForTesting();

			return (WhsReceiveLine)inventory.InDocketLine;
		}

		internal static void AssertContents(OrderLine dataObject)
		{
			AssertEquals("dataObject.Commodity.Code", "HAZ", dataObject.Commodity.Code);
			AssertEquals("dataObject.Commodity.Description", "HAZARDOUS GOODS", dataObject.Commodity.Description);
			AssertEquals("dataObject.ExpectedQuantity", 11.1m, dataObject.ExpectedQuantity);
			AssertEquals("dataObject.ExpiryDate", new ZDateTime(2011, 1, 1), dataObject.ExpiryDate);
			AssertEquals("dataObject.LineNumber", 3, dataObject.LineNumber);
			AssertEquals("dataObject.OrderedQty", 22.2m, dataObject.OrderedQty);
			AssertEquals("dataObject.OrderedQtyUnit.Code", "NO", dataObject.OrderedQtyUnit.Code);
			AssertEquals("dataObject.OrderedQtyUnit.Description", "Number", dataObject.OrderedQtyUnit.Description);
			AssertEquals("dataObject.CrossDockOrderNumber", "REC1", dataObject.CrossDockOrderNumber);
			AssertEquals("dataObject.PackageQty", 22.2m, dataObject.PackageQty);
			AssertEquals("dataObject.PackageQtyUnit.Code", "CTN", dataObject.PackageQtyUnit.Code);
			AssertEquals("dataObject.PackageQtyUnit.Description", "Carton", dataObject.PackageQtyUnit.Description);
			AssertEquals("dataObject.PackingDate", new ZDateTime(2011, 1, 2), dataObject.PackingDate);
			AssertEquals("dataObject.PalletID", "PALLET1", dataObject.PalletID);
			AssertEquals("dataObject.PartAttribute1", "Wide", dataObject.PartAttribute1);
			AssertEquals("dataObject.PartAttribute2", "Long", dataObject.PartAttribute2);
			AssertEquals("dataObject.PartAttribute3", "Thick", dataObject.PartAttribute3);
			AssertEquals("dataObject.SerialNumber", "SERN", dataObject.SerialNumber);
			AssertEquals("dataObject.Product.Code", "ROLLDOLL", dataObject.Product.Code);
			AssertEquals("dataObject.Product.Description", "Roll this doll on the soil", dataObject.Product.Description);
			AssertEquals("dataObject.ReservedQuantity", null, dataObject.ReservedQuantity);
			AssertEquals("dataObject.SplitQuantity", null, dataObject.SplitQuantity);
			AssertEquals("dataObject.Status.Code", "PND", dataObject.Status.Code);
			AssertEquals("dataObject.Status.Description", "Pending Receipt", dataObject.Status.Description);
			AssertEquals("dataObject.SubLineNumber", 4, dataObject.SubLineNumber);
			AssertEquals("dataObject.OriginalHoldCode.Code", "HEL", dataObject.OriginalHoldCode.Code);
			AssertEquals("dataObject.OriginalHoldCode.Description", "Held", dataObject.OriginalHoldCode.Description);
			AssertEquals("dataObject.CurrentHoldReason", "Washed my car", dataObject.CurrentHoldReason);
		}

		protected override WhsReceiveLine GetNewDocketLine()
		{
			return GetReceiveLine(Factory);
		}

		protected override WhsReceiveLineDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO)
		{
			return new WhsReceiveLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)));
		}
		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
