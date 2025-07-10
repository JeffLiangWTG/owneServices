namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Commodity_DutyTaxFeeTest : NX5105Commodity_DutyTaxFeeAbstractTest<NX5105Commodity_DutyTaxFee>
	{
		protected override NX5105Commodity_DutyTaxFee GetDutyTaxFee(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			return new NX5105Commodity_DutyTaxFee(entryLine, invoiceLine);
		}
	}
}
