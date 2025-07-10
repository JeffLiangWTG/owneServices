using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNHTSAAdditionalNumber)]
	public class USNHTSAAdditionalNumAddInfo : AutoUSNHTSAAdditionalNumAddInfo
	{
		public USNHTSAAdditionalNumAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);

			US_NHTAdditionalIdentityNumQualifierInfo.HumanReadableName = "Number Type";
			US_NHTAdditionalIdentityNumberInfo.HumanReadableName = "Number";
		}

		public NHTSAAdditionalNum AdditionalNum => Parent as NHTSAAdditionalNum;
	}
}
