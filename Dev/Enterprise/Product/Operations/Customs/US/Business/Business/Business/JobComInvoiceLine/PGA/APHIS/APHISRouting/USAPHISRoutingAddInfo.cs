using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAPHISRouting)]
	public class USAPHISRoutingAddInfo : AutoUSAPHISRoutingAddInfo
	{
		public USAPHISRoutingAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new APHISRouting Parent
		{
			get { return (APHISRouting)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
