using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USLaceyCountries)]
	public class LaceyCountryAddInfo : AutoUSCountriesAddInfo
	{
		public LaceyCountryAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
