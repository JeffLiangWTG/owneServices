using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader)]
	public class USDeliveryOrderHeaderAddInfo : AutoUSDeliveryOrderHeaderAddInfo
	{
		public USDeliveryOrderHeaderAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new DeliveryOrderHeader Parent
		{
			get { return (DeliveryOrderHeader)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
