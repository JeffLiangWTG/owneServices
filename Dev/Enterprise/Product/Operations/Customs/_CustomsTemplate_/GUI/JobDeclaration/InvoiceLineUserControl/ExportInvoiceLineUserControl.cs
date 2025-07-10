namespace Enterprise.Customs._CustomsTemplate_.GUI
{
	public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}
	}
}
