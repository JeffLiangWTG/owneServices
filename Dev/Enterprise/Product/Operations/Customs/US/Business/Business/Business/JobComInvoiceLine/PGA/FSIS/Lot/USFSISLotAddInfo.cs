using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USFSISLot)]
	public class USFSISLotAddInfo : AutoUSFSISLotAddInfo
	{
		public USFSISLotAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new USFSISLot Parent
		{
			get { return (USFSISLot)base.Parent; }
		}
	}
}
