
namespace Enterprise.Customs.SE.GUI
{
	public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}
	}
}
