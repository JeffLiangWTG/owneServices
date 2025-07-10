using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business
{
	sealed class ShipmentToOrderFilterMapHelperTest : TestCaseWithFactory
	{
		public void TestLastCompletedMilestoneIsMappedToOrder()
		{
			AssertWorkflowModuleFilterIsMappedToOrder("Last Completed Milestone");
		}

		public void TestMilestoneDateIsMappedToOrder()
		{
			AssertWorkflowModuleFilterIsMappedToOrder("Milestone Date");
		}

		public void TestNextMilestoneIsMappedToOrder()
		{
			AssertWorkflowModuleFilterIsMappedToOrder("Next Milestone");
		}

		void AssertWorkflowModuleFilterIsMappedToOrder(string filterName)
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var shipmentMilestoneFilter = (WorkflowModuleFilter)shipmentFilter[filterName];
			shipmentMilestoneFilter.IsActive = true;
			shipmentMilestoneFilter.Property1 = new ZDateTime(2007, 1, 1, 0, 0, 0);
			shipmentMilestoneFilter.Property2 = new ZDateTime(2009, 1, 1, 0, 0, 0);
			shipmentMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var mapper = new ShipmentToOrderFilterMapHelper(shipmentFilter, ordersFilter);
			mapper.MapFilters();

			var orderMilestoneFilter = (WorkflowModuleFilter)ordersFilter[filterName];

			CombineAssertions(() =>
			{
				AssertEquals(true, orderMilestoneFilter.IsActive);
				AssertEquals(shipmentMilestoneFilter.Property1, orderMilestoneFilter.Property1);
				AssertEquals(shipmentMilestoneFilter.Property2, orderMilestoneFilter.Property2);
				AssertEquals(shipmentMilestoneFilter.PropertySearch, orderMilestoneFilter.PropertySearch);
			});
		}

		public void TestMilestoneCompletedIsMappedToOrder()
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var shipmentMilestoneFilter = (WorkflowModuleTextFilter)shipmentFilter["Milestone Completed"];
			shipmentMilestoneFilter.IsActive = true;
			shipmentMilestoneFilter.Property = "Value";

			var mapper = new ShipmentToOrderFilterMapHelper(shipmentFilter, ordersFilter);
			mapper.MapFilters();

			var ordersMilestoneFilter = (WorkflowModuleTextFilter)ordersFilter["Milestone Completed"];

			CombineAssertions(() =>
			{
				AssertEquals(true, ordersMilestoneFilter.IsActive);
				AssertEquals(shipmentMilestoneFilter.Property, ordersMilestoneFilter.Property);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			ordersFilter = new OrdersFilterBusinessObject();
		}
		OrdersFilterBusinessObject ordersFilter;
	}
}
