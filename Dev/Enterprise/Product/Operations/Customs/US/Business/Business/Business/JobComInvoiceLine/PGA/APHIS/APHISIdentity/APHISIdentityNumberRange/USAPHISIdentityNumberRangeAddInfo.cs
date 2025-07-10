using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAPHISIdentityNumberRange)]
	public class USAPHISIdentityNumberRangeAddInfo : AutoUSAPHISIdentityNumberRangeAddInfo
	{
		public USAPHISIdentityNumberRangeAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new APHISIdentityNumberRange Parent
		{
			get { return (APHISIdentityNumberRange)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
