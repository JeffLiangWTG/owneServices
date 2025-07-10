using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class PrePostCarriageLocationWrapperExtensions
	{
		public static IPrePostCarriageLocation FallbackIfEmpty(this IPrePostCarriageLocation value, ZString fallbackValue)
		{
			return value != null && !value.IsEmpty ? value : new PrePostCarriageLocationWrapper(fallbackValue);
		}

		public static IPrePostCarriageLocation FallbackIfEmpty(this IPrePostCarriageLocation value, ISupportWebAddressValidation fallbackAddress, string fallbackTransportMode)
		{
			return value != null && !value.IsEmpty ? value : new PrePostCarriageLocationWrapper(fallbackAddress, fallbackTransportMode);
		}

		public static IPrePostCarriageLocation FallbackIfEmpty(this IPrePostCarriageLocation value, IPrePostCarriageLocation fallbackValue)
		{
			return value != null && !value.IsEmpty ? value : fallbackValue;
		}
	}
}

