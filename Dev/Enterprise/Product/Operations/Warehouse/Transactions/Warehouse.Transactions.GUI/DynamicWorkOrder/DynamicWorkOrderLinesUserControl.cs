using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class DynamicWorkOrderLinesUserControl : ZUserControl
	{
		public DynamicWorkOrderLinesUserControl()
		{
			InitializeComponent();
		}

		public void SetVisibilityForAssembly(bool isAssembly)
		{
			ParentLinesGridControl.SetVisibilityForAssembly(isAssembly);
			ComponentLinesGroupBox.Visible = isAssembly;
		}
	}
}

