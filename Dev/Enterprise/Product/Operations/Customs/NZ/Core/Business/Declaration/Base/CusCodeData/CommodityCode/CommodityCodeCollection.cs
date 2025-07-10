using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityCodeCollection : Customs.Business.CusCodeDataCollection<CommodityCode>
	{
		public CommodityCodeCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.NZTSWCommodityData)
		{
		}
	}
}
