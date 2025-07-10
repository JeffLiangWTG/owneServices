using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityCollection : DependentCusAddInfoCollection<CommodityLine, JobComInvoiceLine>
	{
		public NZCommodityCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityData)
		{
		}
	}
}
