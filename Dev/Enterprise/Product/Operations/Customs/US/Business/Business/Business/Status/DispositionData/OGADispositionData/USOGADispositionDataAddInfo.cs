using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USOGADisposition)]
	public class USOGADispositionDataAddInfo : AutoUSOGADispositionDataAddInfo
	{
		public USOGADispositionDataAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new OGADispositionData Parent
		{
			get { return (OGADispositionData)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
