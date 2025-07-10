using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData)]
	public class NZCommodityItineraryAddInfo : AutoNZCommodityItineraryAddInfo
	{
		public new class Schema : AutoNZCommodityItineraryAddInfo.Schema
		{
		}

		public NZCommodityItineraryAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new JobComInvoiceLine Parent { get; internal set; }

		public AutoCommodityItinerary CommodityItinerary
		{
			get;
			set;
		}
	}
}
