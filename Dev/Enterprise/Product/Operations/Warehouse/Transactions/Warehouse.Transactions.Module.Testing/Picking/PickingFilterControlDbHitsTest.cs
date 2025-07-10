using System;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class PickingFilterControlDbHitsTest : PickingOrReleaseFilterControlDbHitsTest<PickingFilterBusinessObject>
	{
		protected override PickingFilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PickingFilterBusinessObject();
		}

		protected override ZFilterStripControl GetNewFilterControl(WhsPickCollection collection, PickingFilterBusinessObject filterBizO)
		{
			return new PickingFilterControl(collection, filterBizO);
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var pickings = new WhsPickCollection(Factory);
			var filterBizO = new PickingFilterBusinessObject();
			return new PickingFilterControl(pickings, filterBizO);
		}

		public void TestTaskPlanningStatusColumn()
		{
			var taskPlanningStatus = nameof(WhsPick.WP_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(taskPlanningStatus);
				AssertNotNull("Task Planning Status column", column);
				Assert("Task Planning Status column should be unavailable", column.IsUnavailable);
			}

			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(taskPlanningStatus);
				AssertNotNull("Task Planning Status column", column);
				Assert("Task Planning Status column should not be unavailable", !column.IsUnavailable);
			}
		}

		public void TestDistributionCentreColumn()
		{
			using (var filterControl = GetNewFilterStripControl())
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(nameof(WhsPick.DistributionCentreCodes));
				AssertNotNull("Distribution Center column", column);
				Assert("Distribution Center column should be shown by default", column.IsVisible);
			}
		}
	}
}
