using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityItineraryCollection : DependentCusAddInfoCollection<CommodityItinerary, JobComInvoiceLine>
	{
		public NZCommodityItineraryCollection(JobComInvoiceLine master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData)
		{
		}
	}
}
