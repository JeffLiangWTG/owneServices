using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class GroupInvoiceUserControl : Customs.GUI.BaseInvoiceGroupingUserControl
	{
		public GroupInvoiceUserControl()
		{
			InitializeComponent();
			GroupChargeGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, USCustomsSupplierHeaderUserControl.IsCIFComponent);
			GroupChargeGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_PrepaidCollect);
		}
	}
}
