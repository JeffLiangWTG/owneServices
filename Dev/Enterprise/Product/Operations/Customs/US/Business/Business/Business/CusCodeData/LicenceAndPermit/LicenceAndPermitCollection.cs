namespace Enterprise.Customs.US.Business
{
	public class LicenceAndPermitCollection : Customs.Business.CusCodeDataCollection<LicenceAndPermit>
	{
		public LicenceAndPermitCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.LicenceAndPermit)
		{
		}
	}
}
