using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class ICO2ePrePostCarriageExtentions
	{
		public static IEnumerable<PrePostCarriageLegWrapper> GetPreCarriageLegs(this ICO2ePrePostCarriage provider, ZString hblDeliveryMode = default)
		{
			return GetPrePostCarriageLegsFromLocations(true, provider.GetPreCarriageLocations(hblDeliveryMode));
		}

		public static IEnumerable<PrePostCarriageLegWrapper> GetPostCarriageLegs(this ICO2ePrePostCarriage provider, ZString hblDeliveryMode = default)
		{
			return GetPrePostCarriageLegsFromLocations(false, provider.GetPostCarriageLocations(hblDeliveryMode));
		}

		static IEnumerable<PrePostCarriageLegWrapper> GetPrePostCarriageLegsFromLocations(bool isPre, params IPrePostCarriageLocation[] locations)
		{
			var notEmptyLocations = locations
				.Where(location => !location.IsEmpty)
				.ToArray();

			return notEmptyLocations
				.Zip(notEmptyLocations.Skip(1), (from, to) => new PrePostCarriageLegWrapper(from, to, isPre ? from.TransportMode : to.TransportMode))
				.Where(leg => !leg.From.Equals(leg.To));
		}
	}
}

