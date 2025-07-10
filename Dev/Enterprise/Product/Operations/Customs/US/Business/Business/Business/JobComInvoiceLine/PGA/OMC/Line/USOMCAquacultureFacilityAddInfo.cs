using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USOMCDetails)]
	public class USOMCAquacultureFacilityAddInfo : AutoUSOMCAquacultureFacilityAddInfo
	{
		public USOMCAquacultureFacilityAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new OMCHeader Parent
		{
			get { return (OMCHeader)base.Parent; }
		}
	}
}
