using System;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public static class CO2eLegBasedDataTransferHelper
	{
		public static bool IsCO2eResponseApplicable(this ICO2eLegBasedSupporter topLevelBO, UniversalShipment dataObject)
		{
			if (topLevelBO == null)
			{
				return false;
			}

			if (topLevelBO.GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent)
			{
				return false;
			}

			return true;
		}

		public static string ConvertTransportModeForCO2eCalculation(this ICO2eLegBasedSupporter supporter)
		{
			var mode = supporter.TransportMode;
			switch (mode)
			{
				case TransportModes.Sea:
				case TransportModes.Air:
				case TransportModes.Rail:
				case TransportModes.Road:
					return mode;
				case TransportModes.SeaAir:
					return TransportModes.Sea;
				case TransportModes.AirSea:
					return TransportModes.Air;
				case TransportModes.Courier:
					return ((supporter as IImportExport)?.IsDomestic() ?? false) ? TransportModes.Road : TransportModes.Air;
				default:
					throw new ArgumentException($"Invalid {supporter.GetType().Name} transport mode for Greenhouse Gas Emissions");
			}
		}

		public static string ConvertContainerModeForCO2eCalculation(this ICO2eLegBasedSupporter supporter)
		{
			var transportMode = supporter.ConvertTransportModeForCO2eCalculation();
			var containerMode = supporter.ContainerMode;
			if (transportMode == TransportModes.Sea || transportMode == TransportModes.Road || transportMode == TransportModes.Rail)
			{
				if (containerMode == ContainerModes.BuyersConsol)
				{
					return ContainerModes.FCL;
				}
				else if (containerMode == ContainerModes.ShippersConsol)
				{
					return ContainerModes.FCL;
				}
				else if (containerMode == ContainerModes.Groupage)
				{
					return ContainerModes.FCL;
				}
				else if (containerMode == ContainerModes.Other)
				{
					return ContainerModes.Bulk;
				}
			}
			else if (transportMode == TransportModes.Air)
			{
				if (containerMode == ContainerModes.Other)
				{
					return ContainerModes.Loose;
				}
				else if (containerMode == ContainerModes.BuyersConsol)
				{
					return ContainerModes.ULD;
				}
			}
			return containerMode;
		}
	}
}
