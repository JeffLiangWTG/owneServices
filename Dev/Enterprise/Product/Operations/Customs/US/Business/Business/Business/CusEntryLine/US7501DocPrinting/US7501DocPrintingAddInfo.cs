using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.US7501DocPrinting)]
	public class US7501DocPrintingAddInfo : AutoUS7501DocPrintingAddInfo
	{
		public US7501DocPrintingAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new US7501DocPrinting Parent
		{
			get { return (US7501DocPrinting)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
