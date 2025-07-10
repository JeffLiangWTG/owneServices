using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityProductDataCollection : Customs.Business.CusCodeDataCollection<CommodityProductData>
	{
		public CommodityProductDataCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData)
		{
		}
	}
}
