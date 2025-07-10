using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityConstituentCollection : DependentCusAddInfoCollection<CommodityConstituent, JobComInvoiceLine>
	{
		public NZCommodityConstituentCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityConstituentData)
		{
		}
	}
}
