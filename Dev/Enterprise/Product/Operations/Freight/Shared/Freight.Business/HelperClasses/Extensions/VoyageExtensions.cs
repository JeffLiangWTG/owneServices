
#if DEBUG

namespace Enterprise.Freight.Business.Extensions
{
	public static class VoyageExtensions
	{
		public static JobVoyage With(this JobVoyage voyage, string jV_AirSeaRoad = null, string jV_RV_NKVessel = null, string jV_VoyageFlight = null, string jV_VoyageType = null)
		{
			voyage.JV_AirSeaRoad = jV_AirSeaRoad;
			voyage.JV_RV_NKVessel = jV_RV_NKVessel;
			voyage.JV_VoyageFlight = jV_VoyageFlight;
			voyage.JV_VoyageType = jV_VoyageType;

			return voyage;
		}
	}
}

#endif
