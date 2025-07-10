using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZTSWCommodityData)]
	public class NZCommodityAddInfo : AutoNZCommodityAddInfo
	{
		public new class Schema : AutoNZCommodityAddInfo.Schema
		{
		}

		public NZCommodityAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new JobComInvoiceLine Parent { get; internal set; }

		public AutoCommodityLine CommodityLine
		{
			get;
			set;
		}
	}
}
