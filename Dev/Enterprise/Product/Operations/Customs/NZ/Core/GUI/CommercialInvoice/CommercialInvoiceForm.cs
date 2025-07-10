using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration;

namespace Enterprise.Customs.NZ.GUI.CommercialInvoice
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
			return new CustomsInvoiceLineUserControl();
		}

		#region Designer Generated Code
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		readonly System.ComponentModel.Container components;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
		#endregion
	}
}


