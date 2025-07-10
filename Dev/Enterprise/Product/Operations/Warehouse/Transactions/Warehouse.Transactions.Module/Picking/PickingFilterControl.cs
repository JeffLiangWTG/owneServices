using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class PickingFilterControl : ZFilterStripControl
	{
		public PickingFilterControl()
		{
			InitializeComponent();
			grid.SetAvailability(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value, WhsPick.Schema.WP_TaskPlanningStatus);
		}

		public PickingFilterControl(IBusinessObjectCollection gridCollection, PickingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			grid.SetAvailability(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value, WhsPick.Schema.WP_TaskPlanningStatus);
		}
	}
}
