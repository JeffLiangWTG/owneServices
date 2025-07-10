using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDOTVIN)]
	public class DOTVINAddInfo : AutoUSDOTVINAddInfo
	{
		public DOTVINAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new DOTVIN Parent => base.Parent as DOTVIN;
	}
}
