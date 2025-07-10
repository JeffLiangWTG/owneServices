using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsInventoryHoldChangeLog))]
	class WhsInventoryHoldChangeLogTest : WhsBusinessObjectTestCase
	{
		#region Implementation

		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete dbo.WhsInventoryHoldChangeLog", false, GetNewBusinessObject().CanDelete);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var clientOrgHeader = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("TST");
			var receive = Helper.CreateWhsReceive(clientOrgHeader, warehouse);
			Factory.Save();

			var b_2 = warehouse.DefaultInboundDockDoorLocation;
			var docketLine = Helper.CreateWhsReceiveLine(receive, Helper.CreateProduct(clientOrgHeader, "JJJ"), 1m, b_2);

			var result = Factory.New<WhsInventoryHoldChangeLog>();

			result.WHL_WE_ParentDocketLine = docketLine.PK;
			result.WHL_LogVersion = 1;
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override bool IsDeleteSupported() => false;

		#endregion
	}
}
