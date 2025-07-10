using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAPHISProduct)]
	public class USAPHISProductAddInfo : AutoUSAPHISProductAddInfo
	{
		public USAPHISProductAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new APHISProduct Parent
		{
			get { return (APHISProduct)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
