using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public static class ShipmentExtensions
	{
		public static ForwardingConsol ConsolForCountry(this ForwardingShipment shipment, string countryCode)
		{
			ForwardingConsol result = null;
			if (shipment != null)
			{
				foreach (Transport transport in shipment.TransportsIncludingRelated)
				{
					if (CountryCode(transport.JW_RL_NKDiscPort) == countryCode && transport.Parent is ForwardingConsol)
					{
						return (ForwardingConsol)transport.Parent;
					}
				}
			}

			return result;
		}

		#region Implementation

		static ZString CountryCode(ZString unloco)
		{
			return unloco.SubstringSafe(0, 2);
		}

		#endregion
	}
}
