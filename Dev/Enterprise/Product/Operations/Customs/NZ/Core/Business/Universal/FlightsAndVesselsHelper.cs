using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business
{
	public static class FlightsAndVesselsHelper
	{
		public static bool IsValidFlightOrVessel(BusinessObjectFactory factory, ZString flightVessel)
		{
			var result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, flightVessel, Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZFlightsAndVessels, ZDateTime.UtcNow);
			return result != null;
		}
	}
}
