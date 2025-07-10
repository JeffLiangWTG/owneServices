using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USSCI)]
	public class USScientificDataAddInfo : AutoUSScientificDataAddInfo
	{
		public USScientificDataAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new ScientificData Parent
		{
			get { return (ScientificData)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
