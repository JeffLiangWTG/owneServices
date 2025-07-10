using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.GUI.CommercialInvoice
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		public CommercialInvoiceForm() { }

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl()
		{
			return new InvoiceHeaderUserControl();
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			return new InvoiceLinesUserControl();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}

