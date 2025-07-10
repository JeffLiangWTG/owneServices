using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData)]
	public class NZCommodityProductAddInfo : AutoNZCommodityProductAddInfo
	{
		public new class Schema : AutoNZCommodityProductAddInfo.Schema
		{
		}

		public NZCommodityProductAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new JobComInvoiceLine Parent { get; internal set; }

		public AutoCommodityProduct CommodityProduct
		{
			get;
			set;
		}
	}
}
