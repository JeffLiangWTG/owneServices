using System;
using System.Linq;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickingFilterBusinessObject))]
	class PickingFilterBusinessObjectTest : PickingOrReleaseFilterBusinessObjectTestCase<PickingFilterBusinessObject>
	{
		#region TestFilterPickingTaskPlanningStatus

		public void TestFilterPickingTaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 6m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 7m);

			var pick1 = Helper.CreatePickNew(order1);

			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var pick4 = Helper.CreatePickNew(order4);
			pick4.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			Factory.Save();

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["PickTaskPlanningStatus"];
				filter.Property = TaskPlanningStatus.Codes.NotReady;
				filter.IsActive = true;
				AssertEquals("PickTaskPlanningStatus filter category should be StatusAndFlags.", filter.Category, FilterCategories.StatusAndFlags);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals(1, picks.Length);
				AssertEquals(pick2, picks.Single());

				filter.Property = TaskPlanningStatus.Codes.Ready;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals(1, picks.Length);
				AssertEquals(pick3, picks.Single());

				filter.Property = TaskPlanningStatus.Codes.Planned;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals(1, picks.Length);
				AssertEquals(pick4, picks.Single());

				filter.Property = "";
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals(4, picks.Length);
			}
		}

		#endregion
	}
}
