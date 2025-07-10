using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public static class AgencyUNDGHelper
	{
		public static ZBool CFRShouldBeDefaulted(CommonShipment shipment)
		{
			var origin = shipment?.Origin?.Country?.Code;
			var destination = shipment?.Destination?.Country?.Code;

			return CFRShouldBeDefaulted(origin, destination);
		}

		public static ZBool CFRShouldBeDefaulted(ZString? origin, ZString? destination)
		{
			if (!origin.HasValue)
			{
				return false;
			}

			if (!destination.HasValue)
			{
				return origin.Value.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedStates);
			}

			return origin.Value.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedStates) || IsCanadaToUS(origin.Value, destination.Value);
		}

		static bool IsCanadaToUS(ZString origin, ZString destination)
		{
			return (origin.EqualsIgnoringCase(Core.Constants.CountryCodes.Canada) && destination.EqualsIgnoringCase(Core.Constants.CountryCodes.UnitedStates));
		}
	}
}
