using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USITDoc)]
	public class USITDocAddInfo : AutoUSITDocAddInfo
	{
		public USITDocAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
