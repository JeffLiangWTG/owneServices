using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNHTSAPermitAndLicense)]
	public class USNHTSAPermitAndLicenseAddInfo : AutoUSNHTSAPermitAndLicenseAddInfo
	{
		public USNHTSAPermitAndLicenseAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);

			US_NHTLPCOTypeInfo.HumanReadableName = "LPCO Type";
			US_NHTLPCONumberInfo.HumanReadableName = "LPCO Number";
			US_NHTLPCODateTypeInfo.HumanReadableName = "LPCO Date Type";
			US_NHTLPCODateInfo.HumanReadableName = "LPCO Date";
			US_NHTLPCOQuantityInfo.HumanReadableName = "LPCO Quantity";
		}

		public NHTSAPermitAndLicenses PermitAndLicenses => Parent as NHTSAPermitAndLicenses;
	}
}
