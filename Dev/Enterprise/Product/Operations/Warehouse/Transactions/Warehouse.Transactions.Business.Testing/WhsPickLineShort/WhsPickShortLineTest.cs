using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickShortLine))]
	public class WhsPickShortLineTest : WhsBusinessObjectTestCase
	{
		#region Related Entities

		public void TestTransactionLine()
		{
			var shortLine = Factory.New<WhsPickShortLine>();
			AssertNull(shortLine.TransactionLine);

			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			shortLine.WZS_WE_TransactionLine = orderLine.PK;
			AssertEquals(orderLine, shortLine.TransactionLine);
			AssertEquals(typeof(WhsOrderLine), shortLine.TransactionLine.GetType());

			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			shortLine.WZS_WE_TransactionLine = workOrderLine.PK;
			AssertEquals(workOrderLine, shortLine.TransactionLine);
			AssertEquals(typeof(WhsWorkOrderLine), shortLine.TransactionLine.GetType());
		}

		public void TestInventoryLine()
		{
			var shortLine = Factory.New<WhsPickShortLine>();
			AssertNull(shortLine.InventoryLine);

			var transfer = Factory.New<WhsTransfer>();
			var transferLine = transfer.Lines.AddNew();
			shortLine.WZS_WE_InventoryLine = transferLine.PK;
			AssertEquals(transferLine, shortLine.InventoryLine);
			AssertEquals(typeof(WhsTransferLine), shortLine.InventoryLine.GetType());

			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			shortLine.WZS_WE_InventoryLine = receiveLine.PK;
			AssertEquals(receiveLine, shortLine.InventoryLine);
			AssertEquals(typeof(WhsReceiveLine), shortLine.InventoryLine.GetType());
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsPickShortLine", expected: false, GetNewBusinessObject().CanDelete);
		}

		#endregion

		#region BizO Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var org1 = Helper.CreateClient("444", "444");
			var part1 = Helper.CreateProduct(org1, "P1");
			var receive = Helper.CreateWhsReceive(org1, whs);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, part1, 10m);
			var order = Helper.CreateWhsOrder(org1, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, part1, 10m);
			Factory.Save();

			var result = Factory.New<WhsPickShortLine>();
			result.WZS_WE_InventoryLine = receiveLine.PK;
			result.WZS_WE_TransactionLine = orderLine.PK;
			result.WZS_ShortUnits = 10m;
			result.WZS_GS_NKShortedBy = "~BP";
			result.WZS_ShortedDateTimeUtc = DateTime.Now;
			return result;
		}

		protected override bool IsDeleteSupported() => false;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
			=> GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
			=> GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
			=> GetNewBusinessObject();

		#endregion
	}
}
