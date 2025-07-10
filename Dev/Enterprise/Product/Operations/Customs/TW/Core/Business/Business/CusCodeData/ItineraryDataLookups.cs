using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ItineraryDataLookups : CusCodeDataLookups
	{
		public ItineraryDataLookups(AutoCusCodeData parent) : base(parent)
		{
		}

		public new RefCountryCollection CY_CodeList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.ItineraryDataLookups.CY_CodeList", () => new RefCountryCollection(Factory));
	}
}
