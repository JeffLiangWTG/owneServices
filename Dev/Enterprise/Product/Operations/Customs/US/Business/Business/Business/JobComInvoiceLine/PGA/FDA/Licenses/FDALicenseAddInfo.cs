using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USFDALicense)]
	public class FDALicenseAddInfo : AutoFDALicenseAddInfo
	{
		public FDALicenseAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new FDALicense Parent
		{
			get { return (FDALicense)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
