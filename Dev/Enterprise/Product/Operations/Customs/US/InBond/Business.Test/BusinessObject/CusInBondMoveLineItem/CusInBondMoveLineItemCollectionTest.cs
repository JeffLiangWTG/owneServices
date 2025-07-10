using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveLineItemCollection))]
	sealed class CusInBondMoveLineItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveLineItemCollection>
	{
		public void TestDefaultBondedWhsDataFor7512Document()
		{
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var address2 = warehouse.Addresses.AddNew();
			var whsWarehouse = helper.GetNewWhsWarehouse(address2.PK, true, "W#@", Warehouse.Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone);
			var moveDetail = SetupData();
			moveDetail.MoveHeader.BM_OA_WarehouseAddress = address2.PK;
			AssertEquals(0, moveDetail.CBP7512Lines.Count);
			moveDetail.CBP7512Lines.DefaultBondedWhsDataFor7512Document();
			AssertEquals(12, moveDetail.CBP7512Lines.Count);
			var warehouseData = @"| 
 
MERCHANDISE IS FOREIGN TRADE
MERCHANDISE - SEE BELOW
 
|0||0
MARKS 3|40 PS DESC 3|0||0
|PRODUCT:~~1|0||0
|2010.10.1010|15|LB|3011
|1010.10.1010|9|KG|3012
|PRODUCT:~~2|0||0
|2020.20.1010|268|LB|3020
MARKS 1|10 NO DESC 1|0||0
|2020.20.1010|152|KG|1001
|1010.10.1010|101|LB|1002
MARKS 2|20 CS DESC 2
PRODUCT:~~2|0||0
|1010.10.1010|150|KG|2000";
			AssertMultilineASCIIEquals("Data", warehouseData, GetResult(moveDetail.CBP7512Lines));
			var header = moveDetail.Header;
			header.BH_FTZMove = ZBool.False;
			moveDetail.CBP7512Lines.DeleteAll();
			AssertEquals(0, moveDetail.CBP7512Lines.Count);
			var firstLine = moveDetail.CBP7512Lines.AddNew();
			firstLine.BI_MarksAndNumbers = "MARKS NEW";
			firstLine.BI_Description = "DESC NEW";
			firstLine.BI_Weight = 123m;
			firstLine.BI_WeightUnit = "KD";
			firstLine.BI_MonetaryValue = 150m;
			header.BH_FTZMove = ZBool.True;
			moveDetail.CBP7512Lines.DefaultBondedWhsDataFor7512Document();
			AssertEquals(13, moveDetail.CBP7512Lines.Count);
			var extraData = "MARKS NEW|DESC NEW|123|KD|150";
			AssertMultilineASCIIEquals("Data", extraData + System.Environment.NewLine + warehouseData, GetResult(moveDetail.CBP7512Lines));
		}

		public void TestContainerMarkingNoMarks()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "KHDF234";
			bill.B0_ManifestQty = 70;
			bill.B0_ManifestUQ = "DD";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "ENT2KHDF234";
			warehouseDetail.US_WarehouseBondedQuantity = 200m;
			warehouseDetail.US_WarehouseWithdrawQuantity = 70m;
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TURE32423";
			var commondity1 = container.Commodities.AddNew();
			commondity1.BY_PartNumber = helper.Part.OP_PartNum;
			commondity1.BY_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
			commondity1.BY_Description = "DESC 1";
			commondity1.BY_PieceCount = 10;
			commondity1.BY_ManifestUnitCode = "NO";
			commondity1.BY_WarehouseEntryNumber = "ENT2KHDF234";
			commondity1.BY_WarehouseEntryLineNo = 1;
			commondity1.BY_InvoiceQuantity = 100m;
			AssertEquals(0, moveDetail.CBP7512Lines.Count);
			moveDetail.CBP7512Lines.DefaultBondedWhsDataFor7512Document();
			var warehouseData = GetResult(moveDetail.CBP7512Lines);
			Assert(!string.IsNullOrEmpty(warehouseData));
			Assert(!warehouseData.Contains(Core.Constants.ContainerMarking.NoMarks));
		}

		public void TestAddToDescriptionTrimStart()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "KHDF234";
			bill.B0_ManifestQty = 70;
			bill.B0_ManifestUQ = "DD";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "ENT2KHDF234";
			warehouseDetail.US_WarehouseBondedQuantity = 200m;
			warehouseDetail.US_WarehouseWithdrawQuantity = 70m;
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TURE32423";
			var commondity1 = container.Commodities.AddNew();
			commondity1.BY_PartNumber = helper.Part.OP_PartNum;
			commondity1.BY_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
			commondity1.BY_Description = "DESC 1\r\nFDP";
			commondity1.BY_PieceCount = 10;
			commondity1.BY_ManifestUnitCode = "NO";
			commondity1.BY_WarehouseEntryNumber = "ENT2KHDF234";
			commondity1.BY_WarehouseEntryLineNo = 1;
			commondity1.BY_InvoiceQuantity = 100m;
			moveDetail.CBP7512Lines.DefaultBondedWhsDataFor7512Document();
			var line7512 = moveDetail.CBP7512Lines.FirstOrDefault(x => x.BI_Description.Contains("DESC 1"));
			Assert(line7512.BI_Description.Contains("10 NO DESC 1\r\nFDP PRODUCT:~~1"));
			commondity1.BY_Description = "DESC 1 FDP";
			moveDetail.CBP7512Lines.DeleteAll();
			moveDetail.CBP7512Lines.DefaultBondedWhsDataFor7512Document();
			line7512 = moveDetail.CBP7512Lines.FirstOrDefault(x => x.BI_Description.Contains("DESC 1"));
			Assert(line7512.BI_Description.Contains("10 NO DESC 1 FDP\r\nPRODUCT:~~1"));
		}

		public void TestDefaultingFirstElement()
		{
			var moveDetail = SetupData();
			AssertEquals(0, moveDetail.CBP7512Lines.Count);
			var line = moveDetail.CBP7512Lines.AddNew();
			AssertEquals(1, moveDetail.CBP7512Lines.Count);
			AssertMultilineASCIIEquals("Data", "N/M|70 DD|0|KG|0", GetResult(moveDetail.CBP7512Lines));
		}

		protected override CusInBondMoveLineItemCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return new CusInBondMoveLineItemCollection(moveDetail);
		}

		string GetResult(CusInBondMoveLineItemCollection lines)
		{
			var result = new ZStringBuilder();
			foreach (var line in lines)
			{
				var lineBuilder = new ZStringBuilder(line.BI_MarksAndNumbers);
				lineBuilder.Append(line.BI_Description);
				lineBuilder.Append(line.BI_Weight.ToString());
				lineBuilder.Append(line.BI_WeightUnit);
				lineBuilder.Append(line.BI_MonetaryValue.ToString());
				result.Append(lineBuilder.ToStringWithDelimiterBetweenAppends("|"));
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		CusInBondMoveDetail SetupData()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "W#@");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "KHDF234";
			bill.B0_ManifestQty = 70;
			bill.B0_ManifestUQ = "DD";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "ENT2KHDF234";
			warehouseDetail.US_WarehouseBondedQuantity = 200m;
			warehouseDetail.US_WarehouseWithdrawQuantity = 70m;
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TURE32423";
			var commondity1 = container.Commodities.AddNew();
			commondity1.BY_PartNumber = helper.Part.OP_PartNum;
			commondity1.BY_MarksAndNumbers = "MARKS 1";
			commondity1.BY_Description = "DESC 1";
			commondity1.BY_PieceCount = 10;
			commondity1.BY_ManifestUnitCode = "NO";
			commondity1.BY_WarehouseEntryNumber = "ENT2KHDF234";
			commondity1.BY_WarehouseEntryLineNo = 1;
			commondity1.BY_InvoiceQuantity = 100m;
			var commondity1Child1 = commondity1.ChildCommodities.AddNew();
			commondity1Child1.BY_HarmonisedTariff = "2020201010";
			commondity1Child1.BY_GrossWeight = 0.152m;
			commondity1Child1.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			commondity1Child1.BY_MonetaryValue = 1001m;
			var commondity1Child2 = commondity1.ChildCommodities.AddNew();
			commondity1Child2.BY_HarmonisedTariff = "1010101010";
			commondity1Child2.BY_GrossWeight = 121.528m;
			commondity1Child2.BY_GrossWeightUnit = Core.Constants.Weight.PoundsTroy;
			commondity1Child2.BY_MonetaryValue = 1002m;
			var commondity2 = container.Commodities.AddNew();
			commondity2.BY_PartNumber = helper.Part2.OP_PartNum;
			commondity2.BY_MarksAndNumbers = "MARKS 2";
			commondity2.BY_Description = "DESC 2";
			commondity2.BY_PieceCount = 20;
			commondity2.BY_ManifestUnitCode = "CS";
			commondity2.BY_WarehouseEntryNumber = "ENT2KHDF234";
			commondity2.BY_WarehouseEntryLineNo = 1;
			commondity2.BY_InvoiceQuantity = 200m;
			commondity2.BY_HarmonisedTariff = "1010101010";
			commondity2.BY_GrossWeight = 1500m;
			commondity2.BY_GrossWeightUnit = Core.Constants.Weight.Hectograms;
			commondity2.BY_MonetaryValue = 2000m;
			var commondity3 = container.Commodities.AddNew();
			commondity3.BY_MarksAndNumbers = "MARKS 3";
			commondity3.BY_Description = "DESC 3";
			commondity3.BY_PieceCount = 40;
			commondity3.BY_ManifestUnitCode = "PS";
			var commondity3Child1 = commondity3.ChildCommodities.AddNew();
			commondity3Child1.BY_PartNumber = helper.Part.OP_PartNum;
			commondity3Child1.BY_WarehouseEntryNumber = "ENT2KHDF234";
			commondity3Child1.BY_WarehouseEntryLineNo = 1;
			commondity3Child1.BY_InvoiceQuantity = 300m;
			var commondity3Child1Child = commondity3Child1.ChildCommodities.AddNew();
			commondity3Child1Child.BY_HarmonisedTariff = "2010101010";
			commondity3Child1Child.BY_GrossWeight = 240m;
			commondity3Child1Child.BY_GrossWeightUnit = Core.Constants.Weight.Ounces;
			commondity3Child1Child.BY_MonetaryValue = 3011m;
			var commondity3Child1Child2 = commondity3Child1.ChildCommodities.AddNew();
			commondity3Child1Child2.BY_HarmonisedTariff = "1010101010";
			commondity3Child1Child2.BY_GrossWeight = 8500m;
			commondity3Child1Child2.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			commondity3Child1Child2.BY_MonetaryValue = 3012m;
			var commondity3Child2 = commondity3.ChildCommodities.AddNew();
			commondity3Child2.BY_PartNumber = helper.Part2.OP_PartNum;
			commondity3Child2.BY_WarehouseEntryNumber = "ENT2KHDF234";
			commondity3Child2.BY_WarehouseEntryLineNo = 2;
			commondity3Child2.BY_InvoiceQuantity = 300m;
			commondity3Child2.BY_HarmonisedTariff = "2020201010";
			commondity3Child2.BY_GrossWeight = 268m;
			commondity3Child2.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			commondity3Child2.BY_MonetaryValue = 3020m;
			return moveDetail;
		}
	}
}
