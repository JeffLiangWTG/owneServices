using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override void ChangeControlsVisibilityWhenMessageTypeChanges()
		{
			base.ChangeControlsVisibilityWhenMessageTypeChanges();
			InvoiceChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
			InvoiceChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);
		}
	}
}
