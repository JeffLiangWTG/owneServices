using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrderSummaryLine))]
	sealed class TrackingWhsOrderSummaryLineTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Cases

		#region Schema

		public void TestSchema()
		{
			AssertEquals("ProductPK", TrackingWhsOrderSummaryLine.Schema.ProductPK);
			AssertEquals("ProductCode", TrackingWhsOrderSummaryLine.Schema.ProductCode);
			AssertEquals("ProductDescription", TrackingWhsOrderSummaryLine.Schema.ProductDescription);
			AssertEquals("PacksQuantity", TrackingWhsOrderSummaryLine.Schema.PacksQuantity);
			AssertEquals("PacksUQ", TrackingWhsOrderSummaryLine.Schema.PacksUQ);
			AssertEquals("OrderedQuantity", TrackingWhsOrderSummaryLine.Schema.OrderedQuantity);
			AssertEquals("UQ", TrackingWhsOrderSummaryLine.Schema.UQ);
			AssertEquals("ReservedQuantity", TrackingWhsOrderSummaryLine.Schema.ReservedQuantity);
		}

		#endregion

		#region Properties

		#region ProductPK

		public void TestProductPK()
		{
			AssertEquals(TestOrderLine.WhsOrderLine.SupplierPart.PK, TestSummaryLine.ProductPK);

			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			AssertNotEquals(part.PK, TestSummaryLine.ProductPK);

			TestSummaryLine.ProductPK = part.PK;
			AssertEquals(part.PK, TestSummaryLine.ProductPK);
			AssertEquals(part, TestSummaryLine.SupplierPart);
		}

		public void TestProductPKInfo()
		{
			AssertEquals("ProductPK", TestSummaryLine.ProductPKInfo.Name);
		}

		#endregion

		#region ProductCode

		public void TestProductCode()
		{
			AssertEquals("TSTPART", TestSummaryLine.ProductCode);
		}

		public void TestProductCodeInfo()
		{
			AssertEquals("ProductCode", TestSummaryLine.ProductCodeInfo.Name);
		}

		#endregion

		#region ProductDescription

		public void TestProductDescription()
		{
			AssertEquals("Test Part", TestSummaryLine.ProductDescription);
		}

		public void TestProductDescriptionInfo()
		{
			AssertEquals("ProductDescription", TestSummaryLine.ProductDescriptionInfo.Name);
		}

		#endregion

		#region PacksQuantity

		public void TestPacksQuantity()
		{
			AssertEquals(10m, TestSummaryLine.PacksQuantity);

			TestSummaryLine.PacksQuantity = 20m;
			AssertEquals(20m, TestSummaryLine.PacksQuantity);
		}

		public void TestPacksQuantityInfo()
		{
			AssertEquals("PacksQuantity", TestSummaryLine.PacksQuantityInfo.Name);
		}

		#endregion

		#region PacksUQ

		public void TestPacksUQ()
		{
			AssertEquals("PLT", TestSummaryLine.PacksUQ);

			TestSummaryLine.PacksUQ = "BOX";
			AssertEquals("BOX", TestSummaryLine.PacksUQ);
		}

		public void TestPacksUQInfo()
		{
			AssertEquals("PacksUQ", TestSummaryLine.PacksUQInfo.Name);
		}

		#endregion

		#region OrderedQuantity

		public void TestOrderedQuantity()
		{
			AssertEquals(100m, TestSummaryLine.OrderedQuantity);

			TestSummaryLine.OrderedQuantity = 20m;
			AssertEquals(20m, TestSummaryLine.OrderedQuantity);
		}

		public void TestOrderedQuantityInfo()
		{
			AssertEquals("OrderedQuantity", TestSummaryLine.OrderedQuantityInfo.Name);
		}

		#endregion

		#region UQ

		public void TestUQ()
		{
			AssertEquals("BOX", TestSummaryLine.UQ);

			TestSummaryLine.UQ = "PLT";
			AssertEquals("PLT", TestSummaryLine.UQ);
		}

		public void TestUQInfo()
		{
			AssertEquals("UQ", TestSummaryLine.UQInfo.Name);
		}

		#endregion

		#region ReservedQuantity

		public void TestReservedQuantity()
		{
			TestSummaryLine.ReservedQuantity = 20m;
			AssertEquals(20m, TestSummaryLine.ReservedQuantity);
		}

		public void TestReservedQuantityInfo()
		{
			AssertEquals("ReservedQuantity", TestSummaryLine.ReservedQuantityInfo.Name);
		}

		#endregion

		#region Pack Types

		public void TestPackTypes()
		{
			AssertEquals(TestSummaryLine.PackTypes, new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits());
		}

		#endregion

		public void TestSupplierPart()
		{
			AssertNotNull(TestSummaryLine.SupplierPart);
			AssertEquals(TestSummaryLine.SupplierPart, TestOrderLine.WhsOrderLine.SupplierPart);
		}

		#endregion

		#endregion

		#region Implementation

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "TST";
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "TSTPART";
			part.OP_Desc = "Test Part";
			part.OP_StockKeepingUnit = "BOX";
			OrgPartUnit partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = "BOX";
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 10;

			TrackingWhsOrder testOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			testOrder.WhsOrder.WD_WW_Whs = warehouse.PK;
			TestOrderLine = TrackingHelper.Get(testOrder.WhsOrder.Lines.AddNew());
			TestOrderLine.WhsOrderLine.WE_OP = part.PK;
			TestOrderLine.WhsOrderLine.WE_PackQuantity = 10;
			TestOrderLine.WhsOrderLine.WE_F3_NKPackType = "PLT";

			TestSummaryLine = new TrackingWhsOrderSummaryLine(TestOrderLine);
		}

		TrackingWhsOrderLine TestOrderLine;
		TrackingWhsOrderSummaryLine TestSummaryLine;

		#endregion

		public void DocTypesTest()
		{
			Assert(true);
		}

		#endregion
	}
}
