using System;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI
{
	public partial class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		[Obsolete("Do not call. Only for designer use.")]
		public CommercialInvoiceForm()
		{
		}

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new CommercialInvoiceHeaderUserControl();

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
