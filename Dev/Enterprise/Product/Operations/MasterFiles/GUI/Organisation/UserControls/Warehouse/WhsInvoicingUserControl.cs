using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WhsInvoicingUserControl : ZUserControl
	{
		public WhsInvoicingUserControl()
		{
			InitializeComponent();
			SetDataSourceBinding(ChargeStorageInAdvanceCheckBox, "IsVisibleForBinding", "CompanyData.IsWhsSplitMonthBilling");
		}
	}
}
