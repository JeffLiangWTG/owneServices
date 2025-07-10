using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsVASOrderDataObjectWriterTest : WhsUniversalTestCase
	{
		#region TestGetDataObject

		public void TestGetDataObject()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			data.Whs1.WW_WarehouseName = "SOMEWAREHOUSE";
			data.Whs1.Areas[0].WA_Name = "AREA4SERVICES";

			var vasOrderBO = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], GetOrganizationBO_CRAHOLSYD(Factory.BOFactory));
			vasOrderBO.WVO_JobID = "WVO0001";
			vasOrderBO.WVO_CustomerReferenceNo = "RefNumber";

			var line1 = vasOrderBO.Lines.AddNew();
			line1.WVL_LineNumber = 1;
			line1.WVL_OP_Product = data.Part1.PK;
			line1.WVL_Quantity = 5;
			line1.WVL_PackingDate = today.AddDays(-1);
			line1.WVL_ExpiryDate = today.AddDays(7);

			var line2 = vasOrderBO.Lines.AddNew();
			line2.WVL_LineNumber = 2;
			line2.WVL_OP_Product = data.Part2.PK;
			line2.WVL_Quantity = 10;
			line2.WVL_PartAttrib1 = "PA-1";
			line2.WVL_PartAttrib2 = "PA-2";
			line2.WVL_PartAttrib3 = "PA-3";

			var line3 = vasOrderBO.Lines.AddNew(); // Minimum required data to save
			line3.WVL_Quantity = 1;
			line3.WVL_OP_Product = data.Part1.PK;
			Factory.SaveForTesting();

			var vasOrderDO = new WhsVASOrderDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(vasOrderBO);
			AssertEquals("WarehouseVASOrder [WVO0001]", vasOrderDO.DataContext.GetDataSources());
			AssertEquals("OrderNumber", "RefNumber", vasOrderDO.Order.OrderNumber);
			AssertEquals("Warehouse Name", "SOMEWAREHOUSE", vasOrderDO.Order.Warehouse.Name);
			AssertEquals("Warehouse Code", "1", vasOrderDO.Order.Warehouse.Code);
			AssertEquals("Service Area (Staging Area field)", "AREA4SERVICES", vasOrderDO.Order.StagingArea);
			AssertEquals("Lines", 3, vasOrderDO.Order.OrderLineCollection.Count);

			AssertOrganizationBO_CRAHOLSYD("WarehouseClient", vasOrderDO.OrganizationAddressCollection.Single(), nameof(OrganisationTypes.WarehouseClient));

			var line1DO = vasOrderDO.Order.OrderLineCollection.SingleOrDefault(o => o.LineNumber == 1);
			var line2DO = vasOrderDO.Order.OrderLineCollection.SingleOrDefault(o => o.LineNumber == 2);
			var line3DO = vasOrderDO.Order.OrderLineCollection.SingleOrDefault(o => o.LineNumber == 3);
			AssertNotNull("Precondition", line1DO);
			AssertNotNull("Precondition", line2DO);
			AssertNotNull("Precondition", line3DO);

			AssertEquals("Product.Code", "P1", line1DO.Product.Code);
			AssertEquals("OrderedQty", 5m, line1DO.OrderedQty);
			AssertEquals("PackingDate", today.AddDays(-1), line1DO.PackingDate);
			AssertEquals("ExpiryDate", today.AddDays(7), line1DO.ExpiryDate);

			AssertEquals("Product.Code", "P2", line2DO.Product.Code);
			AssertEquals("OrderedQty", 10m, line2DO.OrderedQty);
			AssertEquals("PartAttribute1", "PA-1", line2DO.PartAttribute1);
			AssertEquals("PartAttribute2", "PA-2", line2DO.PartAttribute2);
			AssertEquals("PartAttribute3", "PA-3", line2DO.PartAttribute3);

			AssertEquals("Product.Code", "P1", line3DO.Product.Code);
			AssertEquals("OrderedQty", 1m, line3DO.OrderedQty);
			AssertEquals("PackingDate", line3DO.PackingDate, ZDate.Empty);
			AssertEquals("ExpiryDate", line3DO.ExpiryDate, ZDate.Empty);
			AssertEquals("PartAttribute1", ZString.Empty, line3DO.PartAttribute1);
			AssertEquals("PartAttribute2", ZString.Empty, line3DO.PartAttribute2);
			AssertEquals("PartAttribute3", ZString.Empty, line3DO.PartAttribute3);
		}

		#endregion

		#region TestGetDataObject_EmptyVASOrder

		public void TestGetDataObject_EmptyVASOrder()
		{
			AssertNoExceptionThrown(() => new WhsVASOrderDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(Factory.New<WhsVASOrder>()));
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
