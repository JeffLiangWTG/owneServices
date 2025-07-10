using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
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
			return new SGInvoiceLineUserControl();
		}
	}
}
