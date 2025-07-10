using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoodsExtensions
{
	static class ForwardingDangerousGoodsExtensions
	{
		#region IsValidForCFRStandard

		public static bool IsValidForCFRStandard(this ForwardingShipment shipment)
		{
			var anyTransportsSuitableForCFR = shipment?
				.TransportsIncludingRelated
				.Cast<Transport>()
				.Any(IsTransportSuitableForCFR) ?? false;

			return anyTransportsSuitableForCFR || IsShipmentSuitableForCFR(shipment);
		}

		public static bool IsTransportSuitableForCFR(Transport transport)
		{
			return transport != null
				&& transport.JW_TransportMode.Equals(Core.Constants.TransportModes.Road)
				&& IsAnyPortInUSOrUSTerritory(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort);
		}

		static bool IsShipmentSuitableForCFR(ForwardingShipment shipment)
		{
			return shipment != null
				&& shipment.Transports.Count == 0
				&& shipment.JS_TransportMode.Equals(Core.Constants.TransportModes.Road)
				&& IsAnyPortInUSOrUSTerritory(shipment.JS_RL_NKOrigin, shipment.JS_RL_NKDestination);
		}

		static bool IsAnyPortInUSOrUSTerritory(params string[] portCodes)
		{
			return Core.Constants.CountryCodes.UsaAndTerritoriesList.Any(country => portCodes.Any(portCode => portCode.StartsWith(country)));
		}

		#endregion

		#region IsValidForJTTStandard

		public static bool IsValidForJTTStandard(this ForwardingShipment shipment)
		{
			if (shipment == null)
			{
				return false;
			}

			var shipmentHasRoadLegsInChineseTerritories = shipment.TransportsIncludingRelated.OfType<Transport>().Any(IsTransportSuitableForJTT);
			return shipmentHasRoadLegsInChineseTerritories;
		}

		public static bool IsTransportSuitableForJTT(Transport transport)
		{
			if (transport == null || transport.JW_TransportMode != Core.Constants.TransportModes.Road)
			{
				return false;
			}

			return ChineseTerritories.Contains(transport.JW_RL_NKLoadPort.SubstringSafe(0, 2))
				|| ChineseTerritories.Contains(transport.JW_RL_NKDiscPort.SubstringSafe(0, 2));
		}

		static IReadOnlyCollection<ZString> ChineseTerritories => chineseTerritories ?? (chineseTerritories = new ZString[]
		{
			Core.Constants.CountryCodes.China,
			Core.Constants.CountryCodes.HongKong,
			Core.Constants.CountryCodes.Taiwan
		});

		[ThreadStatic]
		static IReadOnlyCollection<ZString> chineseTerritories;

		#endregion
	}
}
