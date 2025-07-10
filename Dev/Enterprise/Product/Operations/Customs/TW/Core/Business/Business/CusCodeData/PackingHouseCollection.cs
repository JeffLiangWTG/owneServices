using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PackingHouseCollection : CusCodeDataCollection<PackingHouse>
	{
		public PackingHouseCollection(JobComInvoiceLine jobComInvoiceLine)
		: base(jobComInvoiceLine, CusCodeDataTypeList.Codes.PackingHouses)
		{
		}
	}
}
