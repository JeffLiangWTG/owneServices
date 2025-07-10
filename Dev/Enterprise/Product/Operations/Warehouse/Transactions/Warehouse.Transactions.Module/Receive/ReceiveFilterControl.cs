using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class ReceiveFilterControl : ZFilterStripControl
	{
		public ReceiveFilterControl()
		{
			InitializeComponent();
			grid.SetAvailability(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value, WhsDocket.Schema.WD_TaskPlanningStatus);
		}

		public ReceiveFilterControl(IBusinessObjectCollection gridCollection, ReceiveFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode);
			grid.SetAvailability(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value, WhsDocket.Schema.WD_TaskPlanningStatus);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new WhsWorkflowFilterStrip();
		}
	}
}
