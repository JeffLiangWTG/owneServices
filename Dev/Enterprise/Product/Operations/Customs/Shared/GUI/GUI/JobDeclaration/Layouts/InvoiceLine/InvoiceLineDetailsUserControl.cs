using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class InvoiceLineDetailsUserControl : ZUserControl
	{
		public InvoiceLineDetailsUserControl()
		{
			InitializeComponent();
		}

		public void SetInvoiceLineDetailsLayout(IPanelLayoutProvider layout)
		{
			DynamicLineDetailsPanel.UpdateLayout(layout);
		}
	}
}
