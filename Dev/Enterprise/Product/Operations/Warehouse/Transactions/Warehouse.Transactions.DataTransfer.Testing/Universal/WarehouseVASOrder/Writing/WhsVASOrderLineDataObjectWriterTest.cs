using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsVASOrderLineDataObjectWriterTest : WhsUniversalTestCase
	{
		#region TestGetDataObject

		public void TestGetDataObject()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			data.Whs1.Areas[0].WA_Name = "AREA4SERVICES";

			var vasOrderBO = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			vasOrderBO.WVO_JobID = "WVO0001";

			var line1 = vasOrderBO.Lines.AddNew();
			line1.WVL_LineNumber = 1;
			line1.WVL_OP_Product = data.Part1.PK;
			line1.WVL_Quantity = 5;
			line1.WVL_PackingDate = today.AddDays(-1);
			line1.WVL_ExpiryDate = today.AddDays(7);
			line1.WVL_PartAttrib1 = "PA-1";
			line1.WVL_PartAttrib2 = "PA-2";
			line1.WVL_PartAttrib3 = "PA-3";

			var line2 = vasOrderBO.Lines.AddNew(); // Minimum required data to save
			line2.WVL_Quantity = 1;
			line2.WVL_OP_Product = data.Part1.PK;
			Factory.SaveForTesting();

			var line1DO = new WhsVASOrderLineDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(line1);
			AssertNotNull("Precondition", line1DO);

			AssertEquals("LineNumber", 1, line1DO.LineNumber);
			AssertEquals("Product.Code", "P1", line1DO.Product.Code);
			AssertEquals("OrderedQty", 5m, line1DO.OrderedQty);
			AssertEquals("PackingDate", today.AddDays(-1), line1DO.PackingDate);
			AssertEquals("ExpiryDate", today.AddDays(7), line1DO.ExpiryDate);
			AssertEquals("PartAttribute1", "PA-1", line1DO.PartAttribute1);
			AssertEquals("PartAttribute2", "PA-2", line1DO.PartAttribute2);
			AssertEquals("PartAttribute3", "PA-3", line1DO.PartAttribute3);

			var line2DO = new WhsVASOrderLineDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(line2);
			AssertNotNull("Precondition", line2DO);
			AssertEquals("LineNumber", 2, line2DO.LineNumber);
			AssertEquals("Product.Code", "P1", line2DO.Product.Code);
			AssertEquals("OrderedQty", 1m, line2DO.OrderedQty);
			AssertEquals("PackingDate", line2DO.PackingDate, ZDate.Empty);
			AssertEquals("ExpiryDate", line2DO.ExpiryDate, ZDate.Empty);
			AssertEquals("PartAttribute1", ZString.Empty, line2DO.PartAttribute1);
			AssertEquals("PartAttribute2", ZString.Empty, line2DO.PartAttribute2);
			AssertEquals("PartAttribute3", ZString.Empty, line2DO.PartAttribute3);
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
