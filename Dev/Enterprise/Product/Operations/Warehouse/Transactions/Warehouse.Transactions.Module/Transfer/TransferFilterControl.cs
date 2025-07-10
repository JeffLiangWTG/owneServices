using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class TransferFilterControl : ZFilterStripControl
	{
		public TransferFilterControl()
		{
			InitializeComponent();
			ShowOrHideTaskPlanningStatusFilter();
		}

		public TransferFilterControl(IBusinessObjectCollection gridCollection, TransferFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ShowOrHideTaskPlanningStatusFilter();
		}

		void ShowOrHideTaskPlanningStatusFilter()
		{
			grid.SetAvailability(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value, WhsDocket.Schema.WD_TaskPlanningStatus);
		}
	}
}
