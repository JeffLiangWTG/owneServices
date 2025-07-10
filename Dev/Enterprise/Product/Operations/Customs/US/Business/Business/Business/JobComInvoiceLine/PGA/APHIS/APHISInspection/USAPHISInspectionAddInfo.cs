using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAPHISInspection)]
	public class USAPHISInspectionAddInfo : AutoUSAPHISInspectionAddInfo
	{
		public USAPHISInspectionAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new APHISInspection Parent
		{
			get { return (APHISInspection)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
