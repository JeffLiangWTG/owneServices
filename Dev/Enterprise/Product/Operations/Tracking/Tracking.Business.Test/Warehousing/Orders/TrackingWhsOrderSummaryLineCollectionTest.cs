using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrderSummaryLineCollection))]
	sealed class TrackingWhsOrderSummaryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingWhsOrderSummaryLineCollection>
	{
		#region Test Cases

		public void TestConstructors()
		{
			TrackingWhsOrderSummaryLineCollection testCollection = new TrackingWhsOrderSummaryLineCollection(Factory);
			AssertEquals(0, testCollection.Count);

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			part1.OP_Desc = "Part1 Description";
			CreateProductUnit(part1, "BOX", "PLT", 2m);
			CreateProductUnit(part1, "BOX", "CAS", 3m);
			part1.OP_StockKeepingUnit = "BOX";

			OrgSupplierPart part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			part2.OP_Desc = "Part2 Description";
			CreateProductUnit(part2, "BOX", "PLT", 4m);
			part2.OP_StockKeepingUnit = "BOX";

			WhsOrderLine line1 = TestOrder.WhsOrder.Lines.AddNew();
			line1.WE_OP = part1.PK;
			line1.WE_PackQuantity = 10;
			line1.WE_F3_NKPackType = "PLT";

			WhsOrderLine line2 = TestOrder.WhsOrder.Lines.AddNew();
			line2.WE_OP = part1.PK;
			line2.WE_PackQuantity = 20;
			line2.WE_F3_NKPackType = "PLT";

			WhsOrderLine line3 = TestOrder.WhsOrder.Lines.AddNew();
			line3.WE_OP = part1.PK;
			line3.WE_PackQuantity = 20;
			line3.WE_F3_NKPackType = "CAS";

			WhsOrderLine line4 = TestOrder.WhsOrder.Lines.AddNew();
			line4.WE_OP = part1.PK;
			line4.WE_PackQuantity = 20;
			line4.WE_F3_NKPackType = "CAS";
			line4.WE_CustomAttrib1 = "TEST";

			WhsOrderLine line5 = TestOrder.WhsOrder.Lines.AddNew();
			line5.WE_OP = part2.PK;
			line5.WE_PackQuantity = 10;
			line5.WE_F3_NKPackType = "PLT";

			AssertEquals(5, TestOrder.WhsOrder.Lines.Count);

			testCollection = new TrackingWhsOrderSummaryLineCollection(TestOrder.Lines, Factory);
			AssertEquals(3, testCollection.Count);

			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 0m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 40m, "CAS", 120m, "BOX", 0m);
			AssertSummaryLinesContains(testCollection, "PART2", "Part2 Description", 10m, "PLT", 40m, "BOX", 0m);
		}

		public void TestAddTrackingWhsOrderLine()
		{
			TrackingWhsOrderSummaryLineCollection testCollection = new TrackingWhsOrderSummaryLineCollection(Factory);
			AssertEquals(0, testCollection.Count);

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			part1.OP_Desc = "Part1 Description";
			CreateProductUnit(part1, "BOX", "PLT", 2m);
			CreateProductUnit(part1, "BOX", "CAS", 3m);
			part1.OP_StockKeepingUnit = "BOX";

			OrgSupplierPart part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			part2.OP_Desc = "Part2 Description";
			CreateProductUnit(part2, "BOX", "PLT", 4m);
			part2.OP_StockKeepingUnit = "BOX";

			TrackingWhsOrderLine line = TrackingHelper.Get(TestOrder.WhsOrder.Lines.AddNew());
			line.WhsOrderLine.WE_OP = part1.PK;
			line.WhsOrderLine.WE_PackQuantity = 10;
			line.WhsOrderLine.WE_F3_NKPackType = "PLT";
			testCollection.Add(line);
			AssertEquals(1, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 10m, "PLT", 20m, "BOX", 0m);

			line = TrackingHelper.Get(TestOrder.WhsOrder.Lines.AddNew());
			line.WhsOrderLine.WE_OP = part1.PK;
			line.WhsOrderLine.WE_PackQuantity = 20;
			line.WhsOrderLine.WE_F3_NKPackType = "PLT";
			testCollection.Add(line);
			AssertEquals(1, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 0m);

			line = TrackingHelper.Get(TestOrder.WhsOrder.Lines.AddNew());
			line.WhsOrderLine.WE_OP = part1.PK;
			line.WhsOrderLine.WE_PackQuantity = 20;
			line.WhsOrderLine.WE_F3_NKPackType = "CAS";
			testCollection.Add(line);
			AssertEquals(2, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 0m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 20m, "CAS", 60m, "BOX", 0m);

			line = TrackingHelper.Get(TestOrder.WhsOrder.Lines.AddNew());
			line.WhsOrderLine.WE_OP = part1.PK;
			line.WhsOrderLine.WE_PackQuantity = 20;
			line.WhsOrderLine.WE_F3_NKPackType = "CAS";
			line.WhsOrderLine.WE_CustomAttrib1 = "TEST";
			testCollection.Add(line);
			AssertEquals(2, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 0m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 40m, "CAS", 120m, "BOX", 0m);

			line = TrackingHelper.Get(TestOrder.WhsOrder.Lines.AddNew());
			line.WhsOrderLine.WE_OP = part2.PK;
			line.WhsOrderLine.WE_PackQuantity = 10;
			line.WhsOrderLine.WE_F3_NKPackType = "PLT";
			testCollection.Add(line);
			AssertEquals(3, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 0m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 40m, "CAS", 120m, "BOX", 0m);
			AssertSummaryLinesContains(testCollection, "PART2", "Part2 Description", 10m, "PLT", 40m, "BOX", 0m);
		}

		public void TestAddTrackingWhsOrderSummaryLine()
		{
			TrackingWhsOrderSummaryLineCollection testCollection = new TrackingWhsOrderSummaryLineCollection(Factory);
			AssertEquals(0, testCollection.Count);

			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			part1.OP_Desc = "Part1 Description";
			CreateProductUnit(part1, "BOX", "PLT", 2m);
			CreateProductUnit(part1, "BOX", "CAS", 3m);
			part1.OP_StockKeepingUnit = "BOX";

			OrgSupplierPart part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			part2.OP_Desc = "Part2 Description";
			CreateProductUnit(part2, "BOX", "PLT", 4m);
			part2.OP_StockKeepingUnit = "BOX";

			TrackingWhsOrderSummaryLine line = new TrackingWhsOrderSummaryLine(Factory);
			line.ProductPK = part1.PK;
			line.PacksQuantity = 10m;
			line.PacksUQ = "PLT";
			line.OrderedQuantity = 20m;
			line.UQ = "BOX";
			line.ReservedQuantity = 10m;
			testCollection.Add(line);
			AssertEquals(1, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 10m, "PLT", 20m, "BOX", 10m);

			line = new TrackingWhsOrderSummaryLine(Factory);
			line.ProductPK = part1.PK;
			line.PacksQuantity = 20m;
			line.PacksUQ = "PLT";
			line.OrderedQuantity = 40m;
			line.UQ = "BOX";
			line.ReservedQuantity = 10m;
			testCollection.Add(line);
			AssertEquals(1, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 20m);

			line = new TrackingWhsOrderSummaryLine(Factory);
			line.ProductPK = part1.PK;
			line.PacksQuantity = 20m;
			line.PacksUQ = "CAS";
			line.OrderedQuantity = 60m;
			line.UQ = "BOX";
			line.ReservedQuantity = 10m;
			testCollection.Add(line);
			AssertEquals(2, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 20m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 20m, "CAS", 60m, "BOX", 10m);

			line = new TrackingWhsOrderSummaryLine(Factory);
			line.ProductPK = part1.PK;
			line.PacksQuantity = 20m;
			line.PacksUQ = "CAS";
			line.OrderedQuantity = 60m;
			line.UQ = "BOX";
			line.ReservedQuantity = 10m;
			testCollection.Add(line);
			AssertEquals(2, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 20m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 40m, "CAS", 120m, "BOX", 20m);

			line = new TrackingWhsOrderSummaryLine(Factory);
			line.ProductPK = part2.PK;
			line.PacksQuantity = 10;
			line.PacksUQ = "PLT";
			line.OrderedQuantity = 40m;
			line.UQ = "BOX";
			line.ReservedQuantity = 10m;
			testCollection.Add(line);
			AssertEquals(3, testCollection.Count);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 20m);
			AssertSummaryLinesContains(testCollection, "PART1", "Part1 Description", 40m, "CAS", 120m, "BOX", 20m);
			AssertSummaryLinesContains(testCollection, "PART2", "Part2 Description", 10m, "PLT", 40m, "BOX", 10m);
		}

		#endregion

		#region Implementation

		void AssertSummaryLinesContains(TrackingWhsOrderSummaryLineCollection lines, ZString productCode, ZString productDescription, ZDecimal packs, ZString packsUQ, ZDecimal ordered, ZString uQ, ZDecimal reserved)
		{
			bool contains = false;
			foreach (TrackingWhsOrderSummaryLine line in lines)
			{
				if (line.ProductCode == productCode &&
					line.ProductDescription == productDescription &&
					line.PacksQuantity == packs &&
					line.PacksUQ == packsUQ &&
					line.OrderedQuantity == ordered &&
					line.UQ == uQ &&
					line.ReservedQuantity == reserved)
				{
					contains = true;
					break;
				}
			}
			Assert("Summary Line for Product: " + productCode + " could not be found", contains);
		}

		void CreateProductUnit(OrgSupplierPart part, string partUnitType, string parentPartUnitType, ZDecimal partUnitSize)
		{
			OrgPartUnit partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = partUnitType;
			partUnit.OF_ParentPackType = parentPartUnitType;
			partUnit.OF_QuantityInParent = partUnitSize;
		}

		protected override void SetUp()
		{
			base.SetUp();
			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "TST";
			TestOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			TestOrder.WhsOrder.WD_WW_Whs = warehouse.PK;
		}

		TrackingWhsOrder TestOrder;

		protected override TrackingWhsOrderSummaryLineCollection GetCollectionToTest()
		{
			TrackingWhsOrder order = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			return new TrackingWhsOrderSummaryLineCollection(order.Lines, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			TrackingWhsOrderLine line = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>());
			return new TrackingWhsOrderSummaryLine(line);
		}

		#endregion
	}
}
