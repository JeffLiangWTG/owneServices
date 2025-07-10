using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class SlaughterDateCollection : CusCodeDataCollection<SlaughterDate>
	{
		public SlaughterDateCollection(JobComInvoiceLine jobComInvoiceLine)
		: base(jobComInvoiceLine, CusCodeDataTypeList.Codes.SlaughterDates)
		{
		}
	}
}
