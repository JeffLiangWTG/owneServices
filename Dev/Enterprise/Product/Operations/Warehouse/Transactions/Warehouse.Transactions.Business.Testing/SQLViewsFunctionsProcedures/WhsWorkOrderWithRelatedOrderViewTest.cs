using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderWithRelatedOrderViewTest : WhsTestCaseWithFactory
	{
		#region TestView

		public void TestView_StandaloneWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);

			Factory.Save();
			AssertEquals("Pre-condition:", ZGuid.Empty, workOrder.WD_WD_ParentDocket);

			var results = LoadSQLFunction();
			AssertWorkOrderAndRelatedParentPK("Stand-alone work order, ReleatedOrderPK should be empty.", results,
				workOrder.PK, ZGuid.Empty);
		}

		public void TestView_WorkOrderHasParent_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD1", data.Part1, 1m);
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WOR1", data.Part1, 1m);
			workOrder.WD_WD_ParentDocket = order.PK;

			Factory.Save();
			AssertEquals("Pre-condition:", order.PK, workOrder.WD_WD_ParentDocket);

			var results = LoadSQLFunction();
			AssertWorkOrderAndRelatedParentPK(
				"Work Order has a parent docket which is an WhsOrder, RelatedOrderPK should be order's PK.", results,
				workOrder.PK, order.PK);
		}

		public void TestView_WorkOrderHasParent_Order_MultipleLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD1", data.Part1, 1m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 1m);
			order2.WD_WD_ParentDocket = order1.PK;

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WOR1", data.Part1, 1m);
			workOrder1.WD_WD_ParentDocket = order2.PK;

			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WOR2", data.Part1, 1m);
			workOrder2.WD_WD_ParentDocket = workOrder1.PK;

			Factory.Save();
			AssertEquals("Pre-condition:", order1.PK, order2.WD_WD_ParentDocket);
			AssertEquals("Pre-condition:", order2.PK, workOrder1.WD_WD_ParentDocket);
			AssertEquals("Pre-condition:", workOrder1.PK, workOrder2.WD_WD_ParentDocket);

			var results = LoadSQLFunction();
			AssertEquals("Should only contain Work order records.", 2, results.Length);
			AssertWorkOrderAndRelatedParentPK(
				"Work Order has a parent docket which is an WhsOrder, RelatedOrderPK should be order's PK.", results,
				workOrder1.PK, order2.PK);
			AssertWorkOrderAndRelatedParentPK(
				"Work Order has a top level parent docket which is an WhsOrder, RelatedOrderPK should be order's PK.",
				results, workOrder2.PK, order2.PK);
		}

		public void TestView_WorkOrderHasParent_NotAnOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WOR1", data.Part1, 1m);
			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WOR2", data.Part1, 1m);
			workOrder2.WD_WD_ParentDocket = workOrder1.PK;

			Factory.Save();
			AssertEquals("Pre-condition:", ZGuid.Empty, workOrder1.WD_WD_ParentDocket);
			AssertEquals("Pre-condition:", workOrder1.PK, workOrder2.WD_WD_ParentDocket);

			var results = LoadSQLFunction();
			AssertEquals("Should only contain Work order records.", 2, results.Length);
			AssertWorkOrderAndRelatedParentPK("Stand-alone work order, ReleatedOrderPK should be empty.", results,
				workOrder1.PK, ZGuid.Empty);
			AssertWorkOrderAndRelatedParentPK(
				"Work Order has a parent docket but it's not an WhsOrder, RelatedOrderPK should be empty.", results,
				workOrder2.PK, ZGuid.Empty);
		}

		#endregion

		#region Implementation

		void AssertWorkOrderAndRelatedParentPK(string message, DynamicBusinessObject[] results, ZGuid workOrderPK,
			ZGuid expectedParentPK)
		{
			var row = results.Single(r => (ZGuid)r[WhsWorkOrderWithRelatedOrderViewSchema.Constants.PK] == workOrderPK);
			AssertEquals(message, expectedParentPK,
				row[WhsWorkOrderWithRelatedOrderViewSchema.Constants.WRO_RelatedOrderPK]);
		}

		DynamicBusinessObject[] LoadSQLFunction()
		{
			var sql = "SELECT * FROM dbo.WhsWorkOrderWithRelatedOrderView";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			return result.ToArray();
		}

		#endregion
	}
}
