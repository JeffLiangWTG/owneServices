namespace Enterprise.Customs.NO.GUI.Testing
{
	sealed class CommercialInvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new CommercialInvoiceHeaderUserControl();
	}
}
