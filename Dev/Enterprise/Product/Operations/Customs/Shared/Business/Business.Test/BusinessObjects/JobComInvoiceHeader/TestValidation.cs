namespace Enterprise.Customs.Business.Testing
{
	sealed class TestValidation : InvoiceHeaderValidation
	{
		public TestValidation(TestInvoice invoice)
			: base(invoice)
		{
		}

		protected override void CheckJZ_Calc_TNI()
		{
			base.CheckJZ_Calc_TNI();
			((TestInvoice)Parent).CheckJZ_Calc_TNICalled = true;
		}
	}
}
