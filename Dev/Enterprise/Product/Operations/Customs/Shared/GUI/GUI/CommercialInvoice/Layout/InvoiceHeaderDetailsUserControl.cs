using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.CommercialInvoice
{
	public partial class InvoiceHeaderDetailsUserControl : ZUserControl
	{
		public InvoiceHeaderDetailsUserControl()
		{
			InitializeComponent();
		}

		public void SetInvoiceHeaderDetailsLayout(IPanelLayoutProvider layout)
		{
			DynamicHeaderDetailsPanel.UpdateLayout(layout);
		}
	}
}
