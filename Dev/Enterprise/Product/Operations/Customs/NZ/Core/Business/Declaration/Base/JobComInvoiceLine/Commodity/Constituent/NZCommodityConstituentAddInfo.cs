using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZTSWCommodityConstituentData)]
	public class NZCommodityConstituentAddInfo : AutoNZCommodityConstituentAddInfo
	{
		public new class Schema : AutoNZCommodityConstituentAddInfo.Schema
		{
		}

		public NZCommodityConstituentAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new JobComInvoiceLine Parent { get; internal set; }

		public AutoCommodityConstituent CommodityConstituent
		{
			get;
			set;
		}
	}
}
