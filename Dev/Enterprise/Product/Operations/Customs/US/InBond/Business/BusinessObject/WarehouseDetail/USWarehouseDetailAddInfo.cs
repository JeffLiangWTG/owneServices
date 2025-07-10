using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.InBond.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USWarehouseDetail)]
	public class USWarehouseDetailAddInfo : AutoUSWarehouseDetailAddInfo
	{
		public USWarehouseDetailAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new WarehouseDetail Parent
		{
			get { return (WarehouseDetail)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
