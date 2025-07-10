using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PackingDateCollection : CusCodeDataCollection<PackingDate>
	{
		public PackingDateCollection(JobComInvoiceLine jobComInvoiceLine)
		: base(jobComInvoiceLine, CusCodeDataTypeList.Codes.PackingDates)
		{
		}
	}
}
