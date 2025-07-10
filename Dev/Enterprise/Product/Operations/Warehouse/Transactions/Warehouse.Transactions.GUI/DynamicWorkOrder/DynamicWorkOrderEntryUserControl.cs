using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class DynamicWorkOrderEntryUserControl : ZUserControl
	{
		public DynamicWorkOrderEntryUserControl()
		{
			InitializeComponent();
		}

		public void SetVisibilityForAssembly(bool isAssembly)
		{
			DocketLinesGridControl.SetVisibilityForAssembly(isAssembly);
			workOrderDocketLinesGridUserControl2.SetVisibilityForAssembly(isAssembly);
		}
	}
}
