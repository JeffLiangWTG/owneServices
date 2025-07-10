using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderLine))]
	class WhsOrderLine_OldTest : WhsDocketLine_OldTestCase
	{
		public void TestLookups()
		{
			SetupOrderLine();
			AssertEquals("Lookup type", typeof(WhsOrderLineLookups), OrderLine.Lookups.GetType());
		}

		public void TestRelatedInLineForExternalProcessing()
		{
			SetupOrderLine();
			WhsWarehouseTransactionLine line = new WhsWarehouseTransactionLine();
			OrderLine.RelatedInLineForExternalProcessing = line;
			AssertEquals(line, OrderLine.RelatedInLineForExternalProcessing);
		}

		public void TestUnitsMetIsRefreshedAfterPick()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAA");

			var receive = Helper.CreateWhsReceive(org, whs, "1", Notify);
			var inLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			var order1 = Helper.CreateWhsOrder(org, whs, "1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order1, part, 5m);
			AssertEquals(0m, line1.SumOfUnitsMet);
			Factory.Save();

			Helper.CreatePickNew(order1);
			AssertEquals(5m, line1.SumOfUnitsMet);
		}

		#region Implementation

		void SetupOrderLine()
		{
			OrderLine = (WhsOrderLine)GetNewBusinessObject();
			Factory.New<WhsOrder>().Lines.Add(OrderLine);
		}

		protected override Type GetExpectedDocketType()
		{
			return typeof(WhsOrder);
		}

		protected WhsOrderLine OrderLine;

		#endregion
	}
}
