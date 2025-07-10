using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat)]
	public class USDeliveryOrderHazmatAddInfo : AutoUSDeliveryOrderHazmatAddInfo
	{
		public USDeliveryOrderHazmatAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new DeliveryOrderHazmat Parent
		{
			get { return (DeliveryOrderHazmat)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
