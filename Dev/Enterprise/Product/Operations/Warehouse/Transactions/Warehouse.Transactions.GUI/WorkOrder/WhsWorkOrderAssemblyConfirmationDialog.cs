using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class WhsWorkOrderAssemblyConfirmationDialog : ZChildForm
	{
		public WhsWorkOrderAssemblyConfirmationDialog(WhsReceive receiveFromWorkOrder)
			: base(receiveFromWorkOrder)
		{
			InitializeComponent();
			MainStatusBar.Visible = false;
		}
	}
}
