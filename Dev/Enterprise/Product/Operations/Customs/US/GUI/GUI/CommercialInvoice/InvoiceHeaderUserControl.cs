namespace Enterprise.Customs.US.GUI
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
			bool isExport = Invoice.IsExport;
			TariffTypeLabel.Visible = isExport;
			TariffTypeDropEdit.Visible = isExport;
		}
	}
}
