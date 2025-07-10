using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USTTBCigar)]
	public class USTTBCigarAddInfo : AutoUSTTBCigarAddInfo
	{
		public USTTBCigarAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new TTBCigar Parent
		{
			get { return (TTBCigar)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
