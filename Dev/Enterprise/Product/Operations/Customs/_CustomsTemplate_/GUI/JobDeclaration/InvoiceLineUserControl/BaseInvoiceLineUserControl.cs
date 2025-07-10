namespace Enterprise.Customs._CustomsTemplate_.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override bool UseUniversalTariff => false;
	}
}
