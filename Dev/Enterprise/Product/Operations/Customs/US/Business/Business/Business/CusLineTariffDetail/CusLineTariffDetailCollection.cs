namespace Enterprise.Customs.US.Business
{
	public class CusLineTariffDetailCollection : Customs.Business.CusLineTariffDetailCollection<CusLineTariffDetail>
	{
		public CusLineTariffDetailCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public CusLineTariffDetailCollection(CusClassPartPivot pivot)
			: base(pivot)
		{
		}
	}
}
