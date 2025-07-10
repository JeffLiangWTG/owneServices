using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WarehousePackingJobParentEventContextValuesProviderTest : WhsTestCaseWithFactory
	{
		public void TestGetAdditionalEventContextValues_OrderWithoutPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD1");
			order.WD_DocketID = "W0001";
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var package = PackingHelper.CreatePackage(order, "BOX1", 1, Core.Constants.PkgUnit.Box);
			var packingParent = (IPackingParentWithPackableItems)order;
			var eventContext = packingParent.GetAdditionalEventContextValuesFromParent();
			AssertEquals(0, eventContext.ToStringContents(e => e.Key + " - " + e.Value).Length);
		}

		public void TestGetAdditionalEventContextValues_OrderWithPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD2");
			order.WD_DocketID = "W0001";
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var package = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickNo = "P0001";
			var packingParent = (IPackingParentWithPackableItems)order;
			var eventContext = packingParent.GetAdditionalEventContextValuesFromParent();
			AssertMultilineASCIIEquals("GetAdditionalEventContextValues()", @"
OrderNumber - W0001
PickNumber - P0001
".Trim(), eventContext.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestGetAdditionalEventContextValues_TransprotReference()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD2");
			order.WD_DocketID = "W0001";
			order.WD_TransportReference = "TRF001";

			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var package = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickNo = "P0001";
			var packingParent = (IPackingParentWithPackableItems)order;
			var eventContext = packingParent.GetAdditionalEventContextValuesFromParent();
			AssertMultilineASCIIEquals("GetAdditionalEventContextValues()", @"
OrderNumber - W0001
PickNumber - P0001
TransportReference - TRF001
".Trim(), eventContext.ToStringContents(e => e.Key + " - " + e.Value));
		}

		#region PackingHelper

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}