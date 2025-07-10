using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USOGADispositionDetail)]
	public class USOGADispositionDetailAddInfo : AutoUSOGADispositionDetailAddInfo
	{
		public USOGADispositionDetailAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new OGADispositionDetail Parent
		{
			get { return (OGADispositionDetail)base.Parent; }
			protected set { base.Parent = value; }
		}
	}
}
