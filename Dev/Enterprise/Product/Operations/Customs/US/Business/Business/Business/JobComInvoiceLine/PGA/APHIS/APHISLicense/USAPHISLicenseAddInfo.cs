using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USAPHISLicense)]
	public class USAPHISLicenseAddInfo : AutoUSAPHISLicenseAddInfo
	{
		public USAPHISLicenseAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new APHISLicense Parent
		{
			get { return (APHISLicense)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
