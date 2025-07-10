namespace Enterprise.Customs.NL.GUI
{
	public partial class ImportInvoiceLineOrganizationsUserControl : EU.GUI.PlugIn.ImportInvoiceLineOrganizationsUserControl
	{
		public ImportInvoiceLineOrganizationsUserControl()
		{
			InitializeComponent();
			FixLayout();
		}

		void FixLayout()
		{
			Controls.Remove(ConsigneeAddressControl);
			Controls.Remove(ConsignorAddressControl);
		}
	}
}
