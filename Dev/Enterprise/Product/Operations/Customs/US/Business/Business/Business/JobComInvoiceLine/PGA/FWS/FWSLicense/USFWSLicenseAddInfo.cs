using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USFWSLicense)]
	public class USFWSLicenseAddInfo : AutoUSFWSLicenseAddInfo
	{
		public USFWSLicenseAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public new FWSLicense Parent
		{
			get { return (FWSLicense)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
