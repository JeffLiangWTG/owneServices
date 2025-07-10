using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsAdjustmentLineDataObjectWriterTest : WhsDocketLineDataObjectWriterTest<WhsAdjustmentLine, WhsAdjustmentLineDataObjectWriter>
	{
		#region TestBasicAdjustmentLineLevelFieldMappings

		public void TestBasicAdjustmentLineLevelFieldMappings()
		{
			var adjustmentLine = SetupAdjustmentLine(Factory.BOFactory);
			var writer = new WhsAdjustmentLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, adjustmentLine)));
			var adjustmentLineDataObject = writer.GetDataObject(adjustmentLine);

			AssertNotNull("adjustmentLineDataObject", adjustmentLineDataObject);

			CombineAssertions(delegate
			{
				AssertContents(adjustmentLineDataObject);
			});
		}

		#endregion

		#region TestHoldCodes

		public void TestHoldCodes()
		{
			var adjustment = Factory.BOFactory.NewWithValidTestData<WhsAdjustment>();
			var adjustmentLineWithHoldCodeExistInSystem = CreateAdjustmentLine(adjustment, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			var adjustmentLineWithCustomHoldCode = CreateAdjustmentLine(adjustment, InventoryStatus.Codes.Held,
				Helper.CreateInventoryHeldCode("Custom", "Custom hold code").WHC_Code);
			var adjustmentLineWithHoldCodeNotExistInSystem = CreateAdjustmentLine(adjustment, InventoryStatus.Codes.Held, "AAAAA");

			// System defined hold code
			var writerForSystemDefinedHoldCode = new WhsAdjustmentLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, adjustmentLineWithHoldCodeExistInSystem)));
			var lineDataObjectForHoldCodesExistInSystem = writerForSystemDefinedHoldCode.GetDataObject(adjustmentLineWithHoldCodeExistInSystem);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, lineDataObjectForHoldCodesExistInSystem.OriginalHoldCode.Code);
			AssertEquals(InventoryHoldCodes.Descriptions.Damaged, lineDataObjectForHoldCodesExistInSystem.OriginalHoldCode.Description);

			// Custom hold code
			var writerForCustomHoldCodes = new WhsAdjustmentLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, adjustmentLineWithCustomHoldCode)));
			var lineDataObjectForCustomHoldCode = writerForCustomHoldCodes.GetDataObject(adjustmentLineWithCustomHoldCode);
			AssertEquals("Custom", lineDataObjectForCustomHoldCode.OriginalHoldCode.Code);
			AssertEquals("Custom hold code", lineDataObjectForCustomHoldCode.OriginalHoldCode.Description);

			// Hold code is not in system
			var writerForHoldCodesNotExistInSystem = new WhsAdjustmentLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, adjustmentLineWithHoldCodeNotExistInSystem)));
			var lineDataObjectForHoldCodesNotExistInSystem = writerForHoldCodesNotExistInSystem.GetDataObject(adjustmentLineWithHoldCodeNotExistInSystem);
			AssertEquals("AAAAA", lineDataObjectForHoldCodesNotExistInSystem.OriginalHoldCode.Code);
			AssertNull(lineDataObjectForHoldCodesNotExistInSystem.OriginalHoldCode.Description);
		}

		WhsAdjustmentLine CreateAdjustmentLine(WhsAdjustment adjustment, string inventoryStatus, string holdCode)
		{
			var line = adjustment.Lines.AddNew();
			line.WE_OriginalInventoryStatus = inventoryStatus;
			line.WE_WHC_NKOriginalInventoryHeldCode = holdCode;
			return line;
		}

		#endregion

		#region Implementation

		protected override WhsAdjustmentLine GetNewDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			return Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, data.Whs1.DefaultLocation);
		}

		protected override WhsAdjustmentLineDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO)
		{
			return new WhsAdjustmentLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)));
		}

		internal static WhsAdjustmentLine SetupAdjustmentLine(BusinessObjectFactory factory)
		{
			var adjustment = factory.NewWithValidTestData<WhsAdjustment>();
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var helper = new WhsTestHelperFunctions(factory);
			var row = helper.CreateRowAndGenerateLocations(adjustment.Warehouse, "A", 10, 5, 5);
			var location = row.Locations.FindByColumnLevelTray(2, 4, 3);
			var adjustmentLine = adjustment.Lines.AddNew();
			var product = helper.CreateProduct(adjustment.Client, "BOWLHAT");
			product.OP_Desc = "Bowler Hat";
			product.OP_RH_NKCommodityCode = "HAZ";
			product.OP_StockKeepingUnit = "NO";

			adjustmentLine.WE_OP = product.PK;
			adjustmentLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(2011, 1, 1);
			adjustmentLine.WE_ExpiryDate = new ZDate(2011, 1, 2);
			adjustmentLine.WE_F3_NKPackType = "PLT";
			adjustmentLine.WE_LineComment = "SO LARGE";
			adjustmentLine.WE_LineNo = new ZShort(2);
			adjustmentLine.WE_PackingDate = new ZDate(2011, 1, 3);
			adjustmentLine.WE_PalletID = "PALLET~1";
			adjustmentLine.WE_PartAttrib1 = "Zise";
			adjustmentLine.WE_PartAttrib2 = "Siwe";
			adjustmentLine.WE_PartAttrib3 = "Locour";
			adjustmentLine.WE_SerialNumber = "SE4324";
			adjustmentLine.WE_SubLineNo = new ZShort(4);
			adjustmentLine.WE_TransactionQuantity = 22.2m;
			adjustmentLine.WE_WL = location.PK;
			adjustmentLine.WE_ReasonCode = AdjustmentReasonCodesCodeList.Codes.ClientInstructed;
			adjustmentLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			adjustmentLine.CustomsData.WB_AllDutiesAmount = 10m;
			adjustmentLine.CustomsData.WB_VATAmount = 2.3m;

			helper.CreateProductUnit(product, Constants.PkgUnit.Pallet, 1000);

			return adjustmentLine;
		}

		internal static void AssertContents(OrderLine adjustmentLineDataObject)
		{
			AssertEquals("adjustmentLineDataObject.ArrivalDate", new ZDateTimeOffset(2011, 1, 1), adjustmentLineDataObject.ArrivalDate);
			AssertEquals("adjustmentLineDataObject.Commodity.Code", "HAZ", adjustmentLineDataObject.Commodity.Code);
			AssertEquals("adjustmentLineDataObject.Commodity.Description", "HAZARDOUS GOODS", adjustmentLineDataObject.Commodity.Description);
			AssertEquals("adjustmentLineDataObject.ExpiryDate", new ZDate(2011, 1, 2), adjustmentLineDataObject.ExpiryDate);
			AssertEquals("adjustmentLineDataObject.LineComment", "SO LARGE", adjustmentLineDataObject.LineComment);
			AssertEquals("adjustmentLineDataObject.LineNumber", 2, adjustmentLineDataObject.LineNumber);
			AssertEquals("adjustmentLineDataObject.Location.Column", new ZShort(2), adjustmentLineDataObject.Location.Column);
			AssertEquals("adjustmentLineDataObject.Location.Level", new ZShort(4), adjustmentLineDataObject.Location.Level);
			AssertEquals("adjustmentLineDataObject.Location.Tray", new ZShort(3), adjustmentLineDataObject.Location.Tray);
			AssertEquals("adjustmentLineDataObject.Location.Row", "A", adjustmentLineDataObject.Location.Row);
			AssertEquals("adjustmentLineDataObject.OrderedQty", 22.2m, adjustmentLineDataObject.OrderedQty);
			AssertEquals("adjustmentLineDataObject.OrderedQtyUnit.Code", "NO", adjustmentLineDataObject.OrderedQtyUnit.Code);
			AssertEquals("adjustmentLineDataObject.OrderedQtyUnit.Description", "Number", adjustmentLineDataObject.OrderedQtyUnit.Description);
			AssertEquals("adjustmentLineDataObject.PackageQty", 22.2m, adjustmentLineDataObject.PackageQty);
			AssertEquals("adjustmentLineDataObject.PackageQtyUnit.Code", "PLT", adjustmentLineDataObject.PackageQtyUnit.Code);
			AssertEquals("adjustmentLineDataObject.PackageQtyUnit.Description", "Pallet", adjustmentLineDataObject.PackageQtyUnit.Description);
			AssertEquals("adjustmentLineDataObject.PackingDate", new ZDateTime(2011, 1, 3), adjustmentLineDataObject.PackingDate);
			AssertEquals("adjustmentLineDataObject.PalletID", "PALLET~1", adjustmentLineDataObject.PalletID);
			AssertEquals("adjustmentLineDataObject.PartAttribute1", "Zise", adjustmentLineDataObject.PartAttribute1);
			AssertEquals("adjustmentLineDataObject.PartAttribute2", "Siwe", adjustmentLineDataObject.PartAttribute2);
			AssertEquals("adjustmentLineDataObject.PartAttribute3", "Locour", adjustmentLineDataObject.PartAttribute3);

			var expectedSerialNumber = "SE4324";
			AssertEquals("adjustmentLineDataObject.SerialNumber", expectedSerialNumber, adjustmentLineDataObject.SerialNumber);
			AssertEquals("adjustmentLineDataObject.Product.Code", "BOWLHAT", adjustmentLineDataObject.Product.Code);
			AssertEquals("adjustmentLineDataObject.Product.Description", "Bowler Hat", adjustmentLineDataObject.Product.Description);
			AssertEquals("adjustmentLineDataObject.SubLineNumber", 4, adjustmentLineDataObject.SubLineNumber);
			AssertEquals("adjustmentLineDataObject.AdjustmentReason.Code", AdjustmentReasonCodesCodeList.Codes.ClientInstructed, adjustmentLineDataObject.AdjustmentReason.Code);
			AssertEquals("adjustmentLineDataObject.AdjustmentReason.Description", AdjustmentReasonCodesCodeList.Descriptions.ClientInstructed, adjustmentLineDataObject.AdjustmentReason.Description);
			AssertEquals("adjustmentLineDataObject.InventoryStatus.Code", InventoryStatus.Codes.Available, adjustmentLineDataObject.InventoryStatus.Code);
			AssertEquals("adjustmentLineDataObject.InventoryStatus.Description", InventoryStatus.Descriptions.Available, adjustmentLineDataObject.InventoryStatus.Description);
			AssertEquals("adjustmentLineDataObject.CustomsData.AllDutiesAmount", 10m, adjustmentLineDataObject.CustomsData.AllDutiesAmount);
			AssertEquals("adjustmentLineDataObject.CustomsData.VATAmount", 2.3m, adjustmentLineDataObject.CustomsData.VATAmount);
		}
		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		#endregion
	}
}
