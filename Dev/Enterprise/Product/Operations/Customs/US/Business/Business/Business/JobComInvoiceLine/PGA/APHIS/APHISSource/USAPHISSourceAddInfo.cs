using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAPHISSource)]
	public class USAPHISSourceAddInfo : AutoUSAPHISSourceAddInfo
	{
		public USAPHISSourceAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new APHISSource Parent
		{
			get { return (APHISSource)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
