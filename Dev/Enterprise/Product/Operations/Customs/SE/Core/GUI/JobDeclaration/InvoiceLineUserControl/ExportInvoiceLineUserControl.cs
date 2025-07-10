
namespace Enterprise.Customs.SE.GUI
{
	public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}
	}
}
