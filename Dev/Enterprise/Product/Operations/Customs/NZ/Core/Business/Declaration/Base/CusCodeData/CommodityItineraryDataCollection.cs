using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityItineraryDataCollection : Customs.Business.CusCodeDataCollection<CommodityItineraryData>
	{
		public CommodityItineraryDataCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData)
		{
		}
	}
}
