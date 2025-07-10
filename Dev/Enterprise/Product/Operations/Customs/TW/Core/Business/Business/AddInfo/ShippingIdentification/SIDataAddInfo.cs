using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.TWShippingIdentification)]
	[HumanReadableName(PropertyInfoBusinessObjectName = "Parent")]
	public class SIDataAddInfo : AutoSIDataAddInfo
	{
		public SIDataAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
