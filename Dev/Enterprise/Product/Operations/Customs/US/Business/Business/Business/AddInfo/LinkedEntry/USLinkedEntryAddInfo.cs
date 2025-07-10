using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USLinkedEntry)]
	public class USLinkedEntryAddInfo : AutoUSLinkedEntryAddInfo
	{
		public USLinkedEntryAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
			this.linkedEntry = (LinkedEntry)addInfoProperty.BizObj;
		}
		readonly LinkedEntry linkedEntry;

		internal LinkedEntry LinkedEntry
		{
			get { return linkedEntry; }
		}
	}
}
