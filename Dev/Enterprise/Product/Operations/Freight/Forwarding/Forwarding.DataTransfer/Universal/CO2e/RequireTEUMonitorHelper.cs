using System;
using CargoWise.Common;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class RequireTEUMonitorHelper
	{
		public static IDisposable MonitorRequireTEUChanges(UniversalShipment dataObject, ForwardingShipment shipment)
		{
			if (shipment == null
				|| dataObject?.DataContext?.GetMatchingDataTarget(DataContextType.ForwardingShipment) == null
				|| dataObject.PackingLineCollection == null
				|| dataObject.PackingLineCollection.Count == 0)
			{
				return DisposableAction.NoAction;
			}

			return shipment.MonitorRequireTEUChange(new CO2eStatusChangedReason(freeTextReason: (NoResString)"PackLine changed"));
		}
	}
}
