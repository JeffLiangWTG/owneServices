using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class MaxLineNoManagerTest : WhsTestCaseWithFactory
	{
		#region TestCurrentMaxLineNo

		public void TestCurrentMaxLineNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			AssertEquals((ZShort)0, order.MaxLineNoManager.CurrentMaxLineNo);

			WhsOrderLine line = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			AssertEquals((ZShort)1, order.MaxLineNoManager.CurrentMaxLineNo);

			line.WE_LineNo = 24;
			AssertEquals((ZShort)24, order.MaxLineNoManager.CurrentMaxLineNo);
		}

		public void TestCurrentMaxLineNo_ForWorkOrders()
		{
			var data = new TestDataForBOM(Factory);
			data.BOM.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 2m);

			AssertEquals((ZShort)4, workOrder.MaxLineNoManager.CurrentMaxLineNo);

			WhsWorkOrderLine tempLine = workOrder.Lines.AddNew();
			AssertEquals((ZShort)5, workOrder.MaxLineNoManager.CurrentMaxLineNo);

			workOrder.Lines.Delete(tempLine);
			AssertEquals((ZShort)4, workOrder.MaxLineNoManager.CurrentMaxLineNo);
		}

		#endregion

		#region TestLineNoDeleted

		public void TestLineNoDeleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			WhsOrderLine line2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			WhsOrderLine line3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			AssertEquals((ZShort)3, order.MaxLineNoManager.CurrentMaxLineNo);

			order.Lines.Delete(line2);
			AssertEquals((ZShort)3, order.MaxLineNoManager.CurrentMaxLineNo);

			order.Lines.Delete(line3);
			AssertEquals((ZShort)1m, order.MaxLineNoManager.CurrentMaxLineNo);
		}

		#endregion

		#region TestLineNoSet

		public void TestLineNoSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			AssertEquals((ZShort)0, order.MaxLineNoManager.CurrentMaxLineNo);

			// LineNo was set to bigger that CurrentMaxLineNo value
			order.MaxLineNoManager.LineNoSet(5);
			AssertEquals((ZShort)5, order.MaxLineNoManager.CurrentMaxLineNo);

			// LineNo was set to smaller that CurrentMaxLineNo value
			order.MaxLineNoManager.LineNoSet(4);
			AssertEquals((ZShort)5, order.MaxLineNoManager.CurrentMaxLineNo);
		}

		#endregion
	}
}
